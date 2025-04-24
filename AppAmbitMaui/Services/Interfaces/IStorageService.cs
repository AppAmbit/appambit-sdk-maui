using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;

namespace AppAmbit.Services.Interfaces;

internal interface IStorageService
{
    #region Logs

    Task InitializeAsync();

    Task LogEventAsync(LogEntity logEntity);

    Task LogAnalyticsEventAsync(EventEntity analyticsLog);

    Task<List<LogEntity>> GetAllLogsAsync();
    
    Task<List<EventEntity>> GetAllAnalyticsAsync();
    
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

    Task<List<LogEntity>> GetOldest100LogsAsync();

    Task DeleteLogList(List<LogEntity> logs);

    #endregion"
}