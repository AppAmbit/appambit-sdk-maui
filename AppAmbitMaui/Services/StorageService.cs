using AppAmbit.Models;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Interfaces;
using SQLite;

namespace AppAmbit.Services;

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
            await _database.InsertAsync(appSecrets);
        }
    }
    
    public async Task<string?> GetAppId()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.AppId;
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
            AppVersionBuild = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})",
            StackTrace = exception?.StackTrace,
            Description = Truncate(exception?.Message, 80),
            Title =  Truncate(exception?.StackTrace, 80),
            Properties = "No properties",
            Timestamp = DateTime.Now,
            Type = LogType.Crash
        };
        await _database.InsertAsync(log);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
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