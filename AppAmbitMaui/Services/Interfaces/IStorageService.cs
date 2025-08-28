using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;

namespace AppAmbit.Services.Interfaces;

internal interface IStorageService
{
    #region Logs

    Task InitializeAsync();

    Task LogEventAsync(LogEntity logEntity);

    Task LogAnalyticsEventAsync(EventEntity analyticsLog);

    Task SessionBatchAsync(SessionData sessionData);

    Task<List<SessionBatch>> GetAllSessionsAsync();
    Task<List<LogEntity>> GetAllLogsAsync();
    
    Task<List<EventEntity>> GetAllAnalyticsAsync();
    
    Task DeleteAllLogs();
    
    #endregion

    #region Sensetive data
    
    Task SetDeviceId(string? deviceId);

    Task<string?> GetDeviceId();
    
    Task SetUserId(string userId);

    Task<string?> GetUserId();
    
    Task SetUserEmail(string? email);

    Task<string?> GetUserEmail();

    Task SetAppId(string? appId);

    Task<string?> GetAppId();
    
    Task SetSessionId(string sessionId);
    
    Task<string?> GetSessionId();

    Task<string?> GetConsumerId();

    Task SetConsumerId(string consumerId);

    Task<List<LogEntity>> GetOldest100LogsAsync();

    Task DeleteLogList(List<LogEntity> logs);

    Task<List<EventEntity>> GetOldest100EventsAsync();

    Task DeleteEventList(List<EventEntity> logs);

    #endregion"
}