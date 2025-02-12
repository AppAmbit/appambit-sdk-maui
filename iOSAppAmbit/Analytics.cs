using System.Net.Mime;
using iOSAppAmbit.Services;
using iOSAppAmbit.Services.Base;
using Newtonsoft.Json;
using Shared.Models.Analytics;
using Shared.Models.Endpoints;
using SystemConfiguration;
using Microsoft.Extensions.DependencyInjection;


namespace iOSAppAmbit;

public static class Analytics
{
    public static async Task TrackEventAsync(string eventTitle, Dictionary<string, string> data)
    {
        if (HasInternet())
        {
            var storageService = new StorageService();
            var apiService = Core.Services.GetService<IAPIService>();
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
            var logService = new StorageService();
            var log = new AnalyticsLog
            {   
                Id = Guid.NewGuid(),    
                EventTitle = eventTitle,
                Data = JsonConvert.SerializeObject(data)
            };
        
            await logService?.LogAnalyticsEventAsync(log);
        }
    }

    public static bool HasInternet()
    {
        using var reachability = new NetworkReachability("www.google.com");
        NetworkReachabilityFlags flags;
        if (reachability.TryGetFlags(out flags))
        {
            // Check if connected to the internet
            return (flags & NetworkReachabilityFlags.Reachable) != 0
                   && (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;
        }
        return false;
    }
}