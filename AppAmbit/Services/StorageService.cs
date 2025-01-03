using AppAmbit.Models;
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
    }
    
    public void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        _ = LogUnhandledException(unhandledExceptionEventArgs);
    }

    public async Task SetToken(string? token)
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        appSecrets.Token = token;
        await _database.UpdateAsync(appSecrets);
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

    public async Task<string?> GetToken()
    {
        var appSecrets = await _database.Table<AppSecrets>().FirstOrDefaultAsync();
        return appSecrets?.Token;
    }

    public async Task LogUnhandledException(UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
        var log = new Log
        {   
            Id = Guid.NewGuid(),
            AppVersion = AppInfo.Current.VersionString,
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

    public async Task<List<Log>> GetAllLogsAsync()
    {
        return await _database.Table<Log>().ToListAsync();
    }

    public async Task DeleteAllLogs()
    {
        await _database.DeleteAllAsync<Log>();
    }
}