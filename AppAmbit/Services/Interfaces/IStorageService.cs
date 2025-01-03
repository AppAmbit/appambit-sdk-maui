using AppAmbit.Models.Logs;

namespace AppAmbit.Services.Interfaces;

internal interface IStorageService
{
    #region Logs

    Task LogUnhandledException(UnhandledExceptionEventArgs unhandledExceptionEventArgs);

    Task InitializeAsync();

    Task LogEventAsync(Log log);

    Task<List<Log>> GetAllLogsAsync();

    Task DeleteAllLogs();
    
    void OnUnhandledException(object sender, UnhandledExceptionEventArgs e);

    #endregion

    #region Sensetive data

    Task SetToken(string? token);

    Task<string?> GetToken();

    Task SetAppId(string? appId);

    Task<string?> GetAppId();
    
    Task SetSessionId(string sessionId);
    
    Task<string?> GetSessionId();
    
    #endregion"
}