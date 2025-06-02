using System.Diagnostics;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Models.Analytics;
using static AppAmbit.FileUtils;
using Shared.Utils;
using AppAmbit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace AppAmbit;

internal class SessionManager
{
    private static string? _sessionId = null;
    private static bool _isSessionActive = false;
    private static IAPIService? _apiService;
    public static bool IsSessionActive { get => _isSessionActive; }

    internal static void Initialize(IAPIService? apiService)
    {
        _apiService = apiService;
    }

    public static async Task StartSession()
    {
        Debug.WriteLine("StartSession called");
        if (_isSessionActive)
            return;

        var dateUtc = DateUtils.GetUtcNow;
        var apiResponse = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint(dateUtc));

        if (apiResponse?.ErrorType != ApiErrorType.None)
        {
            Debug.WriteLine("StartSession failed - saving locally");
            SaveLocalStartSession(dateUtc);
            _isSessionActive = true;
            return;
        }

        var response = apiResponse?.Data;
        _sessionId = response?.SessionId;
        _isSessionActive = true;
    }

    public static async Task EndSession()
    {
        if (!_isSessionActive)
        {
            return;
        }

        SessionData? endSession = new SessionData
        {
            Id = Guid.NewGuid().ToString(),
            SessionType = SessionType.End,
            SessionId = _sessionId,
            Timestamp = DateUtils.GetUtcNow
        };

        await EndSessionASync(endSession);
    }

    public static async Task SendEndSessionIfExists()
    {
        var file = GetFilePath(GetFileName(typeof(SessionData)));
        Debug.WriteLine($"file:{file}");
        var endSession = await GetSavedSingleObject<SessionData>();
        if (endSession == null)
            return;

        await EndSessionASync(endSession: endSession);
    }

    public static void SaveEndSession()
    {
        try
        {
            var endSession = new SessionData()
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = _sessionId,
                Timestamp = DateUtils.GetUtcNow,
                SessionType = SessionType.End
            };

            var json = JsonConvert.SerializeObject(endSession, new JsonSerializerSettings
            {
                Converters = [new StringEnumConverter()],
                Formatting = Formatting.Indented
            });

            SaveToFile<SessionData>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error in SaveEndSession: " + ex);
        }
    }

    public static async Task RemoveSavedEndSession()
    {
        _ = await GetSavedSingleObject<SessionData>();
    }

    private static async Task EndSessionASync(SessionData endSession)
    {
        var result = await _apiService?.ExecuteRequest<EndSessionResponse>(new EndSessionEndpoint(endSession));
        if (result?.ErrorType != ApiErrorType.None)
        {
            SaveLocalEndSession(endSession);
        }
        _sessionId = null;
        _isSessionActive = false;
    }

    private static void SaveLocalStartSession(DateTime dateUtc)
    {
        var startSession = new SessionData()
        {
            SessionType = SessionType.Start,
            Id = Guid.NewGuid().ToString(),
            Timestamp = dateUtc
        };

        AppendToJsonArrayFile(startSession, "OfflineSessions");
    }

    private static void SaveLocalEndSession(SessionData endSession)
    {
        AppendToJsonArrayFile(endSession, "OfflineSessions");
    }

    public static void ValidateOrInvalidateSession(bool value)
    {
        _isSessionActive = value;
    }
}
