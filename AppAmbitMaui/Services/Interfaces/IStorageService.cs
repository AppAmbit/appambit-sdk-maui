using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;

namespace AppAmbit.Services.Interfaces;

internal interface IStorageService
{
    #region Logs

    Task InitializeAsync();

    Task LogEventAsync(LogTimestamp logTimestamp);

    Task LogAnalyticsEventAsync(AnalyticsLog analyticsLog);

    Task<List<Log>> GetAllLogsAsync();
    
    Task<List<AnalyticsLog>> GetAllAnalyticsAsync();
    
    Task DeleteAllLogs();
    
    #endregion

    #region Sensetive data
    
    Task SetDeviceId(string? token);

    Task<string?> GetDeviceId();
    
    Task SetUserId(string? token);

    Task<string?> GetUserId();
    
    Task SetUserEmail(string? token);

    Task<string?> GetUserEmail();

    Task SetAppId(string? appId);

    Task<string?> GetAppId();
    
    Task SetSessionId(string sessionId);
    
    Task<string?> GetSessionId();
    
    #endregion"
}