using System.Net.Http.Headers;
using iOSAppAmbit.Services;
using iOSAppAmbit.Services.Base;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared.Models.Analytics;
using Shared.Models.App;
using Shared.Models.Endpoints;
using Shared.Models.Logs;
using Shared.Models.Responses;

namespace iOSAppAmbit;

public static class Core
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    
    public static IServiceProvider Services { get; private set; } = null!;

    public static async Task OnStart(string appKey)
    {
        await InitializeServices();

        await InitializeConsumer(appKey);
        
        await StartSession();

        if (Analytics.HasInternet())
        {
            await SendSummaryAndFile();
            await SendAnalytics();
        }
    }

    public static async Task OnResume()
    {
        var appKey = await storageService?.GetAppId();
        await InitializeConsumer(appKey);
        await StartSession();

    }
    
    public static async Task OnSleep()
    {
        await EndSession();
    }

    private static async Task InitializeConsumer(string appKey)
    {
        var appId = await storageService?.GetAppId();
        var deviceId = await storageService.GetDeviceId();
        
        if (appId == null)
        {
            await storageService.SetAppId(appKey);
        }

        if (deviceId == null)
        {
            var id = Guid.NewGuid().ToString();
            await storageService.SetDeviceId(id);
        }
        
        var consumer = new Consumer
        {
            AppVersion = appInfoService.AppVersion,
            DeviceId = await storageService.GetDeviceId(),
            UserId = "1",
            IsGuest = false,
            UserEmail = "test@gmail.com",
            OS = appInfoService.OS,
            Platform = appInfoService.Platform,
            DeviceModel = appInfoService.DeviceModel,
            Country = appInfoService.Country,
            Language = appInfoService.Language,
            AppKey = appKey,
        };
        var registerEndpoint = new RegisterEndpoint(consumer);
        var remoteToken = await apiService?.ExecuteRequest<TokenResponse>(registerEndpoint);
            
        await storageService.SetToken(remoteToken?.Token);
    }

    private static async Task StartSession()
    {
        var response = await apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint());
        storageService?.SetSessionId(response.SessionId);
    }

    private static async Task EndSession()
    {
        var sessionId = await storageService?.GetSessionId();
        var session = new Session { SessionId = sessionId, Timestamp = DateTime.Now };
        await apiService?.ExecuteRequest<string>(new EndSessionEndpoint(session));
    }

    private static async Task SendAnalytics()
    {
        var analytics = await storageService?.GetAllAnalyticsAsync();
        if (analytics.Count == 0)
        {
            return;
        }
        
        foreach (var item in analytics)
        {
            var analyticsReport = new AnalyticsReport()
            {
                EventTitle = item.EventTitle,
                SessionId = await storageService.GetSessionId(),
                Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data)
            };
            var result = await apiService.ExecuteRequest<object>(new SendAnalyticsEndpoint(analyticsReport));
        }
    }
    
    private static async Task SendSummaryAndFile()
    {
        var logs = await storageService?.GetAllLogsAsync();
        if (logs.Count == 0)
        {
            return;
        }
        
        var summary = new LogSummary
        {
            Title = "Test summary",
            DeviceId = await storageService?.GetDeviceId(),
            DeviceModel = appInfoService?.DeviceModel,
            Platform = appInfoService?.Platform,
            AppVersion = appInfoService?.AppVersion,
            CountryISO = appInfoService?.Country,
            Groups = new List<LogGrouping>()
        };

        foreach (var log in logs)
        {
            if (summary.Groups.Count == 0 || summary.Groups.Any(logGrouping => logGrouping.Title != log.Title))
            {
                summary.Groups.Add(new LogGrouping
                {
                    Description = log.Message,
                    Title = log.Message,
                    LogType = GetLogTypeString(log.Type),
                    Count = 1
                });
            }
            else if (summary.Groups.Any(logGrouping => logGrouping.Title == log.Title))
            {
                var logGrouping = summary.Groups.FirstOrDefault(logGrouping => logGrouping.Title == log.Title);
                if (logGrouping != null)
                {
                    logGrouping.Count++;
                }
            }

            switch (log.Type)
            {
                case LogType.Crash:
                    summary.CrashCount++;
                    break;
                case LogType.Debug:
                    summary.DebugCount++;
                    break;
                case LogType.Error:
                    summary.ErrorCount++;
                    break;
                case LogType.Information:
                    summary.InformationCount++;
                    break;
                case LogType.Warning:
                    summary.WarningCount++;
                    break;
            }
        }
        
        var filePath = Path.Combine(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User).FirstOrDefault()?.Path, "logs.txt");
        var jsonString = JsonConvert.SerializeObject(logs);
        await File.WriteAllTextAsync(filePath, jsonString);

        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

        await apiService?.ExecuteRequest<object>(new SendLogsAndSummaryEndpoint(fileContent, summary));
        
        await storageService.DeleteAllLogs();
    }
        
    private static async Task InitializeServices()
    {
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<IAPIService, APIService>(); 
        serviceCollection.AddSingleton<IAppInfoService, AppInfoService>();
        serviceCollection.AddSingleton<IStorageService, StorageService>();

        Services = serviceCollection.BuildServiceProvider();

        apiService = Services.GetService<IAPIService>();
        appInfoService = Services.GetService<IAppInfoService>();
        storageService = Services.GetService<IStorageService>();
        
        await storageService?.InitializeAsync();
    }

    private static string GetLogTypeString(LogType logType)
    {
        var result = "";
        switch (logType)
        {
            case LogType.Error:
                result = "error";
            break;
            case LogType.Crash:
                result = "crash";
                break;
            case LogType.Warning:
                result = "warn";
            break;
            case LogType.Debug:
                result = "debug";
            break;
            case LogType.Information:
                result = "info";
            break;
        }
        return result;
    }
}