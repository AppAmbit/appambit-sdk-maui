using System.Net.Http.Headers;
using System.Text.Json;
using Akavache;
using AppAmbit.Models;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Refit;

namespace AppAmbit;

public static class Core
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    
    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder)
    {
        BlobCache.ApplicationName = "AppAmbit";

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();
        
        return builder;
    }

    public static async Task OnStart(string appId)
    {
        await InitializeServices();
        
        var isRegistered = await IsRegistered(appId);
        if (isRegistered)
        {
            var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
            if (hasInternet)
            {
                await SendSummaryAndFile();
            }
        }
        
        await StartSession();
    }

    public static async Task OnSleep()
    {
        await EndSession();
    }

    private static async Task<bool> IsRegistered(string appKey)
    {
        var localToken = await storageService?.GetToken();

        if (localToken == null)
        {
            await storageService.SetAppId(appKey);
            await storageService.SetDeviceId(Guid.NewGuid().ToString());
            var consumer = new Consumer
            {
                AppVersion = appInfoService.AppVersion,
                DeviceId = await storageService.GetDeviceId(),
                UserId = "",
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
            
            await storageService.SetToken(remoteToken.Token);
            
            return true;
        }
        else
        {
            return true;
        }
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
        
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "logs.txt");
        var jsonString = JsonSerializer.Serialize(logs);
        await File.WriteAllTextAsync(filePath, jsonString);

        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        

        await apiService?.ExecuteRequest<object>(new SendLogsAndSummaryEndpoint(fileContent, summary));
        
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