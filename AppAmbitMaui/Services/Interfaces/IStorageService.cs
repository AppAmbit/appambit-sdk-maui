using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;

namespace AppAmbit.Services.Interfaces;

internal interface IStorageService
{
    #region Logs

    Task LogUnhandledException(UnhandledExceptionEventArgs unhandledExceptionEventArgs);

    Task InitializeAsync();

    Task LogEventAsync(LogTimestamp logTimestamp);

    Task LogAnalyticsEventAsync(AnalyticsLog analyticsLog);

    Task<List<Log>> GetAllLogsAsync();
    
    Task<List<AnalyticsLog>> GetAllAnalyticsAsync();
    
    Task DeleteAllLogs();
    
    void OnUnhandledException(object sender, UnhandledExceptionEventArgs e);

    #endregion

    #region Sensetive data

    Task SetToken(string? token);

    Task<string?> GetToken();
    
    Task SetDeviceId(string? token);

    Task<string?> GetDeviceId();

    Task SetAppId(string? appId);

    Task<string?> GetAppId();
    
    Task SetSessionId(string sessionId);
    
    Task<string?> GetSessionId();
    
    #endregion"
}