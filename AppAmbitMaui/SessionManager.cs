using System.Diagnostics;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Models.Analytics;
using static AppAmbit.FileUtils;
using AppAmbit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace AppAmbit;

internal class SessionManager
{
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;
    private static string? _sessionId { set; get; } = null;
    public static string? GetSessionId() => _sessionId;
    private static bool _isSessionActive = false;
    public static bool IsSessionActive { get => _isSessionActive; }
    private const string OfflineSessionsFile = "OfflineSessions";

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

        DateTime dateUtc = DateTime.UtcNow;
        await SaveLocalStartSession(dateUtc);
        var apiResponse = await _apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint(dateUtc))!;

        if (apiResponse?.ErrorType != ApiErrorType.None)
        {
            Debug.WriteLine("StartSession failed - saving locally");
            _isSessionActive = true;
            return;
        }

        await UpdateOfflineSessionsFile([new SessionData() {
            Id = Guid.NewGuid().ToString(),
            SessionType = SessionType.Start,
            Timestamp = dateUtc
         }]);

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

        DateTime dateUtc = DateTime.UtcNow;

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
        Debug.WriteLine($"EndSession: {file}");
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
                Timestamp = DateTime.UtcNow,
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
        await SaveLocalEndSession(endSession);
        var result = await _apiService?.ExecuteRequest<EndSessionResponse>(new EndSessionEndpoint(endSession));
        if (result?.ErrorType == ApiErrorType.None)
        {
            await UpdateOfflineSessionsFile([endSession]);
        }
        _sessionId = null;
        _isSessionActive = false;
    }

    internal static async Task SaveLocalStartSession(DateTime dateUtc)
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

        var sessionsUnSorted = await GetSaveJsonArrayAsync<SessionData>(OfflineSessionsFile, null) ?? [];

        var sessions = sessionsUnSorted.OrderBy(x => x.Timestamp).ToList();
        if (sessions.Count == 0)
        {
            return;
        }

        if (sessions.Count == 1)
        {
            await SaveOrSendStartEndSession(sessions[0]);
        }
        else
        {
            var batches = BuildSessionBatches(sessions);

            if (batches.Count == 0)
            {
                return;
            }

            var success = await SendBatchAsync(batches);

            if (!success)
            {
                Debug.WriteLine("Unset sessions");
                return;
            }

            Debug.WriteLine($"Sessions sent: {batches.Count}");
            await UpdateOfflineSessionsFile(sessions);
        }
    }

    private static async Task SaveOrSendStartEndSession(SessionData session)
    {
        if (_apiService == null)
        {
            return;
        }

        if (session.SessionType == SessionType.Start)
        {
            var responseStart = await _apiService.ExecuteRequest<SessionResponse>(new StartSessionEndpoint(session.Timestamp))!;

            if (responseStart?.ErrorType == ApiErrorType.None)
            {
                await UpdateOfflineSessionsFile([session]);
            }
        }
        else
        {
            var responseEnd = await _apiService.ExecuteRequest<EndSessionResponse>(new EndSessionEndpoint(session));

            if (responseEnd?.ErrorType == ApiErrorType.None)
            {
                await UpdateOfflineSessionsFile([session]);
            }
        }
    }

    private static async Task<bool> SendBatchAsync(List<SessionBatch> batches)
    {
        if (_apiService == null)
        {
            return false;
        }

        var endpoint = new SessionsPayload { Sessions = batches.ToList() };

        var endpointResult = await _apiService.ExecuteRequest<SessionsPayload>(new SessionBatchEndpoint(endpoint));

        if (endpointResult?.ErrorType != ApiErrorType.None)
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
        var sortedSession = remaining.OrderBy(x => x.Timestamp);

        await UpdateJsonArrayAsync(OfflineSessionsFile, sortedSession);
    }

    public static void ValidateOrInvalidateSession(bool value)
    {
        _isSessionActive = value;
    }
}
