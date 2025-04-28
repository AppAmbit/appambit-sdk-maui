using iOSAppAmbit.Services.Base;
using Shared.Models.Analytics;
using Shared.Models.App;
using Shared.Models.Logs;
using SQLite;

namespace iOSAppAmbit.Services;

internal class StorageService : IStorageService
{
    private SQLiteAsyncConnection _database;
    
    public StorageService()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    public async Task InitializeAsync()
    {
        if (_database is not null)
        {
            return;
        }

        _database = new SQLiteAsyncConnection(AppConstants.DatabasePath, AppConstants.Flags);
        await _database.CreateTableAsync<AppSecrets>();
        await _database.CreateTableAsync<Log>();
        await _database.CreateTableAsync<AnalyticsLog>();
    }
    
    public void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        LogUnhandledException(unhandledExceptionEventArgs);
        Core.OnSleep();
    }

    public async Task SetToken(string? token)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        appSecrets.Token = token;
        await _database.UpdateAsync(appSecrets);
    }
    
    public async Task<string?> GetToken()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.Token;
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
            var res = await _database.InsertAsync(appSecrets);
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

    public async Task LogUnhandledException(UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
        var log = new Log
        {   
            Id = Guid.NewGuid(),
            AppVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString(),
            Message = exception?.StackTrace,
            Timestamp = DateTime.UtcNow,
            Title = exception?.Message,
            Type = LogType.Crash
        };
        await _database.InsertAsync(log);
    }

    public async Task LogEventAsync(Log log)
    {
        await _database.InsertAsync(log);
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