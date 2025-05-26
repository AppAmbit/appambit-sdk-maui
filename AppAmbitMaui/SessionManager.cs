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
    private static bool _isProcessingEndSession = false;
    private static bool _isProcessingStartSession = false;

    public static string? SessionId { get => _sessionId; }
    public static bool IsSessionActive { get => _isSessionActive; }


    internal static void Initialize(IAPIService? apiService, IStorageService? storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }

    public static async Task StartSession()
    {
        Debug.WriteLine("StartSession called");
        if (_isSessionActive)
            return;

        
        if (_isProcessingStartSession)
            return;

        _isProcessingStartSession = true;
        var dateUtc = DateUtils.GetUtcNow;
        var apiResponse = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint(dateUtc));
        _isProcessingStartSession = false;

        if (apiResponse?.ErrorType != ApiErrorType.None)
        {
            Debug.WriteLine("StartSession failed - saving locally.");
            SaveLocalStartSession(dateUtc);
            _isSessionActive = true;
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
        EndSession? endSession = new EndSession
        {
            Id = sessionId,
            Timestamp = DateUtils.GetUtcNow
        };
        await EndSessionASync(endSession);
    }

    public static async Task SendEndSessionIfExists()
    {
        if (_isProcessingEndSession)
            return;

        try
        {
            _isProcessingEndSession = true;
            var file = GetFilePath(GetFileName(typeof(EndSession)));
            Debug.WriteLine($"file:{file}");
            var endSession = await GetSavedSingleObject<EndSession>();
            if (endSession == null)
                return;
            await EndSessionASync(endSession: endSession);
        }
        finally
        {
            _isProcessingEndSession = false;
        }
    }

    public static async Task SaveEndSession()
    {
        try
        {
            // var sessionId = _sessionId ?? await _storageService?.GetSessionId();
            var endSession = new EndSession() { Timestamp = DateUtils.GetUtcNow };
            var json = JsonConvert.SerializeObject(endSession, Formatting.Indented);

            SaveToFile<EndSession>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error in SaveEndSession: " + ex);
        }
    }

    public static async Task RemoveSavedEndSession()
    {
        _ = await GetSavedSingleObject<EndSession>();
    }

    private static async Task EndSessionASync(EndSession endSession)
    {
        var result = await _apiService?.ExecuteRequest<EndSessionResponse>( new EndSessionEndpoint(endSession));
        if (result?.ErrorType != ApiErrorType.None)
        {
            SaveLocalEndSession(endSession);
        }
        _sessionId = null;
        _isSessionActive = false;
    }

    private static void SaveLocalStartSession(DateTime dateUtc)
    {
        var startSession = new StartSession()
        {
            Timestamp = dateUtc
        };

        AppendToJsonArrayFile(startSession);
    }

    private static void SaveLocalEndSession(EndSession endSession)
    {
        AppendToJsonArrayFile(endSession, "OfflineEndSessions.json");
    }
}
