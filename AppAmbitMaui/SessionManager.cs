using System.Diagnostics;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Models.Analytics;
using static AppAmbit.FileUtils;
using Newtonsoft.Json;
using Shared.Utils;
using AppAmbit.Enums;


namespace AppAmbit;

internal class SessionManager
{
    private static string? _sessionId = null;
    private static bool _isSessionActive = false;
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;
    private static EndSession? _currentEndSession = null;

    public static string? SessionId { get => _sessionId; }
    public static bool IsSessionActive { get => _isSessionActive;}


    internal static void                                                                                                                      Initialize(IAPIService? apiService, IStorageService? storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }

    public static async Task StartSession()
    {
        Debug.WriteLine("StartSession called");
        if (_isSessionActive)
        {
            return;
        }

        var apiResponse = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint());
        if (apiResponse?.ErrorType != ApiErrorType.None)
        {
            return;
        }
        var response = apiResponse?.Data;
        _sessionId = response?.SessionId;
        _storageService?.SetSessionId(response!.SessionId);
        _isSessionActive = true;
    }

    public static async Task EndSession()
    {
        if (!_isSessionActive)
        {
            return;
        }

        string? sessionId = await _storageService?.GetSessionId();
        await EndSessionASync(sessionId: sessionId);
    }

    public static async Task SendEndSessionIfExists()
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
        await EndSessionASync(endSession: _currentEndSession);
        _currentEndSession = null;

    }


    public static async Task SaveEndSession()
    {
        var sessionId = _sessionId ?? await _storageService?.GetSessionId();
        var endSession = new EndSession() { Id = sessionId, Timestamp = DateUtils.GetUtcNow };
        var json = JsonConvert.SerializeObject(endSession, Formatting.Indented);

        SaveToFile<EndSession>(json);
    }

   public static async Task RemoveSavedEndSession()
    {
        _ = await GetSavedSingleObject<EndSession>();
    }   

    private static async Task EndSessionASync(string? sessionId = null, EndSession? endSession = null)
    {
        var endpoint = endSession != null
            ? new EndSessionEndpoint(endSession)
            : new EndSessionEndpoint(sessionId!);

        await _apiService?.ExecuteRequest<EndSessionResponse>(endpoint);
        _sessionId = null;
        _isSessionActive = false;
    }
 
}
