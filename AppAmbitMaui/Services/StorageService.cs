using System.Diagnostics;
using AppAmbit.Enums;
using AppAmbit.Models;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using SQLite;

namespace AppAmbit.Services;

internal class StorageService : IStorageService
{
    private SQLiteAsyncConnection _database;

    public async Task InitializeAsync()
    {
        if (_database is not null)
        {
            return;
        }
        Debug.WriteLine($"DatabasePath: {AppConstants.DatabasePath}");
        _database = new SQLiteAsyncConnection(AppConstants.DatabasePath, AppConstants.Flags);
        await _database.CreateTableAsync<AppSecrets>();
        await _database.CreateTableAsync<LogEntity>();
        await _database.CreateTableAsync<EventEntity>();
        await _database.CreateTableAsync<SessionBatch>();
    }

    public async Task SetDeviceId(string? deviceId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();

        if (appSecrets != null)
        {
            appSecrets.DeviceId = deviceId;
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets()
            {
                DeviceId = deviceId
            };

            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task<string?> GetDeviceId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.DeviceId;
    }

    public async Task SetAppId(string? appId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        if (appSecrets != null)
        {
            appSecrets.AppId = appId;
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets
            {
                AppId = appId
            };
            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task<string?> GetAppId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.AppId;
    }

    public async Task SetUserId(string userId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();

        if (appSecrets != null)
        {
            appSecrets.UserId = userId;
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets { UserId = userId };
            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task<string?> GetUserId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.UserId;
    }

    public async Task SetUserEmail(string? userEmail)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();

        if (appSecrets != null)
        {
            appSecrets.UserEmail = userEmail;
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets() { UserEmail = userEmail };
            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task<string?> GetUserEmail()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.UserEmail;
    }

    public async Task SetSessionId(string sessionId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();

        if (appSecrets != null)
        {
            appSecrets.SessionId = sessionId;
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets()
            {
                SessionId = sessionId,
            };

            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task<string?> GetSessionId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.SessionId;
    }

    public async Task<string?> GetConsumerId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.ConsumerId ?? "";
    }

    public async Task SetConsumerId(string consumerId)
    {

        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();

        if (appSecrets != null)
        {
            appSecrets.ConsumerId = consumerId ?? "";
            await _database.UpdateAsync(appSecrets);
        }
        else
        {
            appSecrets = new AppSecrets
            {
                ConsumerId = consumerId
            };
            await _database.InsertAsync(appSecrets);
        }
    }

    public async Task LogEventAsync(LogEntity logEntity)
    {
        await _database.InsertAsync(logEntity);
    }

    public async Task LogAnalyticsEventAsync(EventEntity analyticsLog)
    {
        await _database.InsertAsync(analyticsLog);
    }

    public async Task SessionBatchAsync(SessionData sessionData)
    {
        switch (sessionData.SessionType)
        {
            case SessionType.Start:

                var id = await _database.ExecuteScalarAsync<string>(
                    "SELECT id FROM SessionBatch WHERE ended_at IS NULL ORDER BY started_at LIMIT 1");

                if (id != null)
                {
                    await _database.ExecuteAsync(
                        "UPDATE SessionBatch SET ended_at = ? WHERE id = ?", sessionData.Timestamp, id);

                    Debug.WriteLine("Session start updated");
                }
                
                var sessionBatch = new SessionBatch
                {
                    Id = sessionData.Id,
                    SessionId = sessionData.SessionId,
                    StartedAt = sessionData.Timestamp
                };
                await _database.InsertAsync(sessionBatch);
                break;
            case SessionType.End:
                var existingSession = await _database.Table<SessionBatch>()
                    .Where(sb => sb.EndedAt == null)
                    .FirstOrDefaultAsync();

                if (existingSession != null)
                {
                    existingSession.EndedAt = sessionData.Timestamp;
                    await _database.UpdateAsync(existingSession);
                    Debug.WriteLine("Session end updated");
                }
                else
                {
                    var sessionEnd = new SessionBatch
                    {
                        Id = sessionData.Id,
                        SessionId = sessionData.SessionId,
                        EndedAt = sessionData.Timestamp
                    };
                    await _database.InsertAsync(sessionEnd);
                    Debug.WriteLine("Session end created");
                }
                break;
            default:
                Debug.WriteLine("Unknown session type");
                break;
        }
    }

    public async Task<List<SessionBatch>> GetAllSessionsAsync()
    {
        return await _database.Table<SessionBatch>()
            .OrderBy(session => session.StartedAt)
            .Where(session => session.SessionId != null)
            .Where(session => session.StartedAt != null || session.EndedAt != null)
            .Take(100)
            .ToListAsync();
    }

    public async Task<List<LogEntity>> GetAllLogsAsync()
    {
        return await _database.Table<LogEntity>().ToListAsync();
    }

    public async Task<List<LogEntity>> GetOldest100LogsAsync()
    {
        return await _database.Table<LogEntity>()
            .OrderBy(log => log.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task DeleteLogList(List<LogEntity> logs)
    {
        var ids = logs.Select(log => log.Id).ToList();
        await _database.RunInTransactionAsync(tran =>
        {
            foreach (var id in ids)
            {
                tran.Execute("DELETE FROM LogEntity WHERE Id = ?", id);
            }
        });
    }

    public async Task<List<EventEntity>> GetAllAnalyticsAsync()
    {
        return await _database.Table<EventEntity>().ToListAsync();
    }

    public async Task DeleteAllLogs()
    {
        await _database.DeleteAllAsync<Log>();
    }

    public async Task<List<EventEntity>> GetOldest100EventsAsync()
    {
        return await _database.Table<EventEntity>()
            .OrderBy(log => log.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task DeleteEventList(List<EventEntity> logs)
    {
        var ids = logs.Select(log => log.Id).ToList();
        await _database.RunInTransactionAsync(tran =>
        {
            foreach (var id in ids)
            {
                tran.Execute("DELETE FROM EventEntity WHERE Id = ?", id);
            }
        });
    }

}