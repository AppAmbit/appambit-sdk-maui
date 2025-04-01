using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Analytics
{
    public static async Task GenerateTestEvent()
    {
        await SendOrSaveEvent("Test Event", new Dictionary<string, string>()
        {
            { "Event", "Custom event" }
        });
    }
    
    public static async Task TrackEvent(string eventTitle, Dictionary<string, string> data = null)
    {
        await SendOrSaveEvent(eventTitle, data);
    }
    
    private static async Task SendOrSaveEvent(string eventTitle, Dictionary<string, string> data = null)
    {
        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (hasInternet)
        {
            var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
            
            var analyticsReport = new Models.Analytics.AnalyticsReport()
            {
                EventTitle = eventTitle,
                SessionId = await storageService.GetSessionId(),
                Data = data.ToDictionary(item => item.Key, item => Truncate(item.Key, 125))
            };
            await apiService.ExecuteRequest<object>(new SendAnalyticsEndpoint(analyticsReport));
        }
        else
        {
            var logService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var log = new AnalyticsLog
            {   
                Id = Guid.NewGuid(),    
                EventTitle = eventTitle,
                Data = JsonConvert.SerializeObject(data.ToDictionary(item => item.Key, item => Truncate(item.Key, 125)))
            };
        
            await logService?.LogAnalyticsEventAsync(log);
        }
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

}