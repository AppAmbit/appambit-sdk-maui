using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Analytics
{
    public static async Task TrackEventAsync(string eventTitle, Dictionary<string, string> data)
    {
        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (hasInternet)
        {
            var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
            var analyticsReport = new AnalyticsReport()
            {
                EventTitle = eventTitle,
                SessionId = await storageService.GetSessionId(),
                Data = data
            };
            var result = await apiService.ExecuteRequest<object>(new SendAnalyticsEndpoint(analyticsReport));
        }
        else
        {
            var logService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var log = new AnalyticsLog
            {   
                Id = Guid.NewGuid(),    
                EventTitle = eventTitle,
                Data = JsonConvert.SerializeObject(data)
            };
        
            await logService?.LogAnalyticsEventAsync(log);
            var list = await logService.GetAllAnalyticsAsync();
        }
    }
}