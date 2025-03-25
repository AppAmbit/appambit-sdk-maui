using System.Net.Http.Headers;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AppAmbit;

public static class Core
{
    private static bool _initialized;
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    
    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder, string appKey)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, state) =>
                {
                    Start(appKey);
                });
                android.OnResume(activity =>
                {
                    if (_initialized)
                        OnResume();
                });
                android.OnPause(activity =>
                {
                    OnSleep();
                });
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((application, options) =>
                {
                    Start(appKey);
                    return true;
                });
                ios.WillEnterForeground(application =>
                {
                    OnResume();
                });
                ios.DidEnterBackground(application =>
                {
                    OnSleep();
                });
            });
#endif
        });

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();
        
        return builder;
    }

    private static async Task Start(string appKey)
    {
        await InitializeServices();

        await InitializeConsumer(appKey);
        
        await StartSession();

        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (hasInternet)
        {
            await SendSummaryAndFile();
            await SendAnalytics();
        }
        
        _initialized = true;
    }

    private static async Task OnResume()
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
        await apiService?.ExecuteRequest<string>(new EndSessionEndpoint(sessionId));
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
            var analyticsReport = new Models.Analytics.AnalyticsReport()
            {
                EventTitle = item.EventTitle,
                SessionId = await storageService.GetSessionId(),
                Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data)
            };
            var result = await apiService.ExecuteRequest<object>(new SendAnalyticsEndpoint(analyticsReport));
        }
    }
    
    internal static async Task SendSummaryAndFile()
    {
        var logs = await storageService?.GetAllLogsAsync();
        if (logs.Count == 0)
        {
            return;
        }
        
        var summary = new LogSummary
        {
            DeviceId = await storageService?.GetDeviceId(),
            DeviceModel = appInfoService?.DeviceModel,
            Platform = appInfoService?.Platform,
            CountryISO = appInfoService?.Country,
            Groups = new List<LogGrouping>()
        };

        foreach (var log in logs)
        {
            if (summary.Groups.Count == 0 || summary.Groups.Any(logGrouping => logGrouping.Title != log.Title))
            {
                summary.Groups.Add(new LogGrouping
                {
                    Timestamp = log.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    AppVersionBuild = log.AppVersionBuild,
                    StackTrace = log.StackTrace,
                    Description = log.Description,
                    Title = log.Title, 
                    Properties = log.Properties,
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
        
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "logs.txt");
        var jsonString = JsonSerializer.Serialize(logs);
        await File.WriteAllTextAsync(filePath, jsonString);

        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        

        var result = await apiService?.ExecuteRequest<string>(new SendLogsAndSummaryEndpoint(fileContent, summary));
        
        await storageService.DeleteAllLogs();
    }
        
    private static async Task InitializeServices()
    {
        apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
        appInfoService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>();
        storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
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