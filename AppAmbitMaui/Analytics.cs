using System.Diagnostics;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Shared.Utils;
using Newtonsoft.Json;
using static AppAmbit.AppConstants;
using static AppAmbit.FileUtils;

namespace AppAmbit;

public static class Analytics
{
    internal static bool _isManualSessionEnabled = false;
    private static string? _sessionId = null;
    private static bool _isSessionActive = false;
    private static EndSession? _currentEndSession = null;
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
        _sessionId = response.SessionId;
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
        _sessionId = null;
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

    public static async Task TrackEvent(string eventTitle, Dictionary<string, string>? data = null)
    {
        await SendOrSaveEvent(eventTitle, data);
    }

    public static async void SendEndSessionIfExists()
    {
        if (_currentEndSession == null)
        {
            var file = GetFilePath(GetFileName(typeof(EndSession)));
            Debug.WriteLine($"file:{file}");
            var endSession = await GetSavedSingleObject<EndSession>();
            if (endSession == null)
                return;
            _currentEndSession = endSession;
        }
        await _apiService?.ExecuteRequest<EndSessionResponse>(new EndSessionEndpoint(_currentEndSession));
        _currentEndSession = null;
        _isSessionActive = false;
    }

    private static async Task SendOrSaveEvent(string eventTitle, Dictionary<string, string> data = null)
    {
        if (!ShouldSendEvent)
        {
            return;
        }
        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        data = data?
            .GroupBy(kvp => Truncate(kvp.Key, TrackEventPropertyMaxCharacters))
            .Take(TrackEventMaxPropertyLimit)
            .ToDictionary(
            g => Truncate(g.Key, TrackEventPropertyMaxCharacters),
            g => Truncate(g.First().Value, TrackEventPropertyMaxCharacters)
            );
        eventTitle = Truncate(eventTitle, TrackEventNameMaxLimit);
        if (hasInternet)
        {
            var _event = new Event()
            {
                Name = eventTitle,
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
                Name = eventTitle,
                Data = data,
                CreatedAt = DateTime.Now,
            };

            await storageService?.LogAnalyticsEventAsync(eventEntity);
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    public static async Task SendBatchEvents()
    {
        Debug.WriteLine("SendBatchEvents");
        var eventEntityList = await _storageService.GetOldest100EventsAsync();
        if (eventEntityList?.Count == 0)
        {
            Debug.WriteLine("No events to send");
            return;
        }

        Debug.WriteLine("Sending events in batch");
        var endpoint = new EventBatchEndpoint(eventEntityList);
        var eventsBatchResponse = await _apiService?.ExecuteRequest<EventsBatchResponse>(endpoint);
        Debug.WriteLine($"eventsBatchResponse:{eventsBatchResponse}");
        await _storageService.DeleteEventList(eventEntityList);
        Debug.WriteLine("Events batch sent");
    }

    public static async Task SaveEndSession()
    {
        var sessionId = _sessionId ?? await _storageService?.GetSessionId();
        var endSession = new EndSession() { Id = sessionId, Timestamp = DateUtils.GetUtcNow };
        var json = JsonConvert.SerializeObject(endSession, Formatting.Indented);

        SaveToFile<EndSession>(json);
    }

    internal static async Task RemoveSavedEndSession()
    {
        _ = await GetSavedSingleObject<EndSession>();
    }

    internal static bool ShouldSendEvent
    {
        get
        {
            if (!_isManualSessionEnabled)
                return true;

            return _isSessionActive;
        }
    }
}