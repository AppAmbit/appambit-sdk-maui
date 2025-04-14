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
        
        _database = new SQLiteAsyncConnection(AppConstants.DatabasePath, AppConstants.Flags);
        await _database.CreateTableAsync<AppSecrets>();
        await _database.CreateTableAsync<LogEntity>();
        await _database.CreateTableAsync<EventEntity>();
    }
    
    public async Task SetDeviceId(string? deviceId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        appSecrets.DeviceId = deviceId;
        await _database.UpdateAsync(appSecrets);
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
        appSecrets.UserId = userId;
        await _database.UpdateAsync(appSecrets);
    }

    public async Task<string?> GetUserId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.UserId;
    }

    public async Task SetUserEmail(string userEmail)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        appSecrets.UserEmail = userEmail;
        await _database.UpdateAsync(appSecrets);
    }

    public async Task<string?> GetUserEmail()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.UserEmail;
    }

    public async Task SetSessionId(string sessionId)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        appSecrets.SessionId = sessionId;
        await _database.UpdateAsync(appSecrets);
    }

    public async Task<string?> GetSessionId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.SessionId;
    }
    
    public async Task LogEventAsync(LogEntity logEntity)
    {    
        await _database.InsertAsync(logEntity);
    }

    public async Task LogAnalyticsEventAsync(EventEntity analyticsLog)
    {
        await _database.InsertAsync(analyticsLog);
    }

    public async Task<List<LogEntity>> GetAllLogsAsync()
    {
        return await _database.Table<LogEntity>().ToListAsync();
    }

    public async Task<List<LogEntity>> GetOldest100LogsAsync()
    {
        return await _database.Table<LogEntity>()
            .OrderBy(log => log.Timestamp)
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
}