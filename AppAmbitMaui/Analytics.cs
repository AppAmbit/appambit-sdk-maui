using System.Diagnostics;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using static AppAmbit.AppConstants;

namespace AppAmbit;

public static class Analytics
{
    internal static bool _isManualSessionEnabled = false;
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
        await SessionManager.StartSession();
    }

    public static async Task EndSession()
    {
        await SessionManager.EndSession();
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

    public static async Task TrackEvent(string eventTitle, Dictionary<string, string>? data = null, DateTime? createdAt = null)
    {
        await SendOrSaveEvent(eventTitle, data, createdAt);
    }

    private static async Task SendOrSaveEvent(string eventTitle, Dictionary<string, string>? data = null, DateTime? createdAt = null)
    {
        data = (data ?? new Dictionary<string, string>())
            .GroupBy(kvp => Truncate(kvp.Key, TrackEventPropertyMaxCharacters))
            .Take(TrackEventMaxPropertyLimit)
            .ToDictionary(
            g => Truncate(g.Key, TrackEventPropertyMaxCharacters),
            g => Truncate(g.First().Value, TrackEventPropertyMaxCharacters)
            );

        eventTitle = Truncate(eventTitle, TrackEventNameMaxLimit);

        var eventRequest = new Event()
        {
            Name = eventTitle,
            Data = data
        };

        var response = _apiService != null
            ? await _apiService.ExecuteRequest<object>(new SendEventEndpoint(eventRequest))
            : null;

        if (response?.ErrorType != ApiErrorType.None)
        {
            var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            var eventEntity = new EventEntity()
            {
                Id = Guid.NewGuid(),
                Name = eventTitle,
                Data = data,
                CreatedAt = createdAt != null ? createdAt.Value : DateTime.UtcNow,
                SessionId = SessionManager.SessionId
            };

            if (storageService != null)
            {
                await storageService.LogAnalyticsEventAsync(eventEntity);
            }
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
        if (eventsBatchResponse?.ErrorType == ApiErrorType.NetworkUnavailable)
        {
            Debug.WriteLine($"Batch of unsent events");
            return;
        }

        Debug.WriteLine($"eventsBatchResponse:{eventsBatchResponse}");
        await _storageService.DeleteEventList(eventEntityList);
        Debug.WriteLine("Events batch sent");
    }

    public static void ClearToken()
    {
        _apiService?.SetToken("");
    }

    public async static Task RequestToken()
    {
        await _apiService?.GetNewToken();
    }

    public static void ValidateOrInvaliteSession(bool value)
    {
        SessionManager.ValidateOrInvalidateSession(value);
    }
}