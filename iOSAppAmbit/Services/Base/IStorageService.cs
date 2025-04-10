using Shared.Models.Analytics;
using Shared.Models.Logs;

namespace iOSAppAmbit.Services.Base;

internal interface IStorageService
{
    #region Logs

    Task LogUnhandledException(UnhandledExceptionEventArgs unhandledExceptionEventArgs);

    Task InitializeAsync();

    Task LogEventAsync(Log log);

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
    Task SetUserId(string userId);

    Task<string?> GetUserId();

    Task SetUserEmail(string userEmail);

    Task<string?> GetUserEmail();
    
    Task SetSessionId(string sessionId);
    
    Task<string?> GetSessionId();
    
    #endregion"
}