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
        await _database.CreateTableAsync<LogTimestamp>();
        await _database.CreateTableAsync<AnalyticsLog>();
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
    
    public async Task LogEventAsync(LogTimestamp logTimestamp)
    {    
        await _database.InsertAsync(logTimestamp);
    }

    public async Task LogAnalyticsEventAsync(AnalyticsLog analyticsLog)
    {
        await _database.InsertAsync(analyticsLog);
    }

    public async Task<List<Log>> GetAllLogsAsync()
    {
        return await _database.Table<Log>().ToListAsync();
    }

    public async Task<List<AnalyticsLog>> GetAllAnalyticsAsync()
    {
        return await _database.Table<AnalyticsLog>().ToListAsync();
    }

    public async Task DeleteAllLogs()
    {
        await _database.DeleteAllAsync<Log>();
    }
}