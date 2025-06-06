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
    private static IAPIService? _apiService;
    private static string? _sessionId = null;
    private static DateTime? _dateSessionTest = null;
    private static bool _isSessionActive = false;
    public static bool IsSessionActive { get => _isSessionActive; }
    public static DateTime? DateSessionTest { get => _dateSessionTest; set => _dateSessionTest = value; }
    private const string OfflineSessionsFile = "OfflineSessions";

    internal static void Initialize(IAPIService? apiService)
    {
        _apiService = apiService;
    }

    public static async Task StartSession()
    {
        Debug.WriteLine("StartSession called");
        if (_isSessionActive)
            return;

        DateTime dateUtc = _dateSessionTest ?? DateUtils.GetUtcNow;
        var apiResponse = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint(dateUtc))!;

        if (apiResponse?.ErrorType != ApiErrorType.None)
        {
            Debug.WriteLine("StartSession failed - saving locally");
            await SaveLocalStartSession(dateUtc);
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

        DateTime dateUtc = _dateSessionTest ?? DateUtils.GetUtcNow;

        SessionData? endSession = new SessionData
        {
            Id = Guid.NewGuid().ToString(),
            SessionType = SessionType.End,
            SessionId = _sessionId,
            Timestamp = dateUtc
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
            await SaveLocalEndSession(endSession);
        }
        _sessionId = null;
        _isSessionActive = false;
    }

    private static async Task SaveLocalStartSession(DateTime dateUtc)
    {
        var startSession = new SessionData()
        {
            SessionType = SessionType.Start,
            Id = Guid.NewGuid().ToString(),
            Timestamp = dateUtc
        };

        _ = await GetSaveJsonArrayAsync(OfflineSessionsFile, startSession);
    }

    private static async Task SaveLocalEndSession(SessionData endSession)
    {
        _ = await GetSaveJsonArrayAsync(OfflineSessionsFile, endSession);
    }

    public static async Task SendBatchSessions()
    {
        Debug.WriteLine("Send Sessions...");
        var sessions = await GetSaveJsonArrayAsync<SessionData>(OfflineSessionsFile, null) ?? [];

        if (sessions.Count == 0)
        {
            return;
        }

        var batches = BuildSessionBatches(sessions);

        var success = await SendBatchAsync(batches);

        if (!success)
        {
            Debug.WriteLine("Unset sessions");
            return;
        }
        
        Debug.WriteLine($"Sessions sent: {batches.Count}");
        await UpdateOfflineSessionsFile(sessions);
    }

    private static async Task<bool> SendBatchAsync(List<SessionBatch> batches)
    {
        var endpoint = new SessionsPayload { Sessions = batches.ToList() };

        var endpointResult = await _apiService.ExecuteRequest<SessionsPayload>(new SessionBatchEndpoint(endpoint));

        if (endpointResult.ErrorType != ApiErrorType.None)
        {
            return false;
        }

        return true;
    }

    private static List<SessionBatch> BuildSessionBatches(List<SessionData> sessions)
    {
        var batchSessions = sessions.OrderBy(d => d.Timestamp).Take(200);
        var starts = batchSessions.Where(s => s.SessionType == SessionType.Start).Select(x => x.Timestamp);
        var ends = batchSessions.Where(s => s.SessionType == SessionType.End).Select(x => x.Timestamp);

        return starts.Zip(ends, (start, end) => new SessionBatch
        {
            StartedAt = start,
            EndedAt = end
        }).ToList();
    }

    private static async Task UpdateOfflineSessionsFile(List<SessionData> sessions)
    {
        var remaining = sessions.Skip(200);

        await UpdateJsonArrayAsync(OfflineSessionsFile, remaining);        
    }

    public static void ValidateOrInvalidateSession(bool value)
    {
        _isSessionActive = value;
    }
}
