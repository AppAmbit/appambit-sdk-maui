using System.Diagnostics;
using System.Runtime.CompilerServices;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Analytics
{
    internal static bool _isManualSessionEnabled = false;
    private static bool _isSessionActive = false;
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;

    internal static void Initialize(IAPIService? apiService, IStorageService? storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }
    
    public static void EnableManualSession()
    {
        _isManualSessionEnabled = true;
        Debug.WriteLine("Manual Session enabled");
    }

    public static async Task StartSession()
    {
        Debug.WriteLine("StartSession called");
        if (_isSessionActive)
        {
            return;
        }

        var response = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint());
        _storageService?.SetSessionId(response.SessionId);
        _isSessionActive = true;
    }

    public static async Task EndSession()
    {
        Debug.WriteLine("EndSession called");
        if (!_isSessionActive)
        {
            Debug.WriteLine("Session didn't started");
            return;
        }
        var sessionId = await _storageService?.GetSessionId();
        await _apiService?.ExecuteRequest<EndSessionResponse>(new EndSessionEndpoint(sessionId));
        _isSessionActive = false;
    }

    public static async void SetUserId(string userId)
    {
        await _storageService.SetUserId(userId);
    }

    public static async Task<string?> GetUserId()
    {
        return await _storageService.GetUserId();
    }

    public static async void SetUserEmail(string userEmail)
    {
        await _storageService.SetUserEmail(userEmail);
    }

    public static async Task<string?> GetUserEmail()
    {
        return await _storageService.GetUserEmail();
    }

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
            var _event = new Event()
            {
                Name = Truncate(eventTitle, 125),
                Data = data
            };
            await _apiService.ExecuteRequest<object>(new SendEventEndpoint(_event));
        }
        else
        {
            var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var eventEntity = new EventEntity()
            {
                Id = Guid.NewGuid(),
                Name = Truncate(eventTitle, 125),
                Data = data
            };

            await storageService?.LogAnalyticsEventAsync(eventEntity);
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}