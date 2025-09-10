using System.Diagnostics;
using SQLite;
using SQLitePCL;

namespace AppAmbitTestingApp.Utils
{
    public sealed class StorableApp
    {
        private const string SessionsTable = "SessionEntity";
        private const string LogsTable = "LogEntity";
        private const string EventsTable = "EventEntity";

        private static readonly Lazy<StorableApp> _lazy = new(() => new StorableApp());
        public static StorableApp Shared => _lazy.Value;

        private SQLiteAsyncConnection _db;
        private readonly SemaphoreSlim _mutex = new(1, 1);

        private const int MaxAttempts = 12;
        private const int BaseDelayMs = 20;
        private const int MaxDelayMs = 1200;
        private static readonly Random _rng = new();

        private StorableApp() { }

        public async Task InitializeAsync()
        {
            if (_db != null) return;

            Batteries_V2.Init();

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "AppAmbit.db3");
            var flags =
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.FullMutex;

            _db = new SQLiteAsyncConnection(databasePath, flags, storeDateTimeAsTicks: false);

            // PRAGMAs for multi-connection coexistence
            _ = await _db.ExecuteScalarAsync<string>("PRAGMA journal_mode=WAL;").ConfigureAwait(false);
            await _db.ExecuteAsync("PRAGMA synchronous=NORMAL;").ConfigureAwait(false);

            await ExecRetryAsync("PRAGMA foreign_keys=ON;");
            await ExecRetryAsync("PRAGMA busy_timeout=15000;");
        }

        public async Task CloseAsync()
        {
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_db != null)
                {
                    await _db.CloseAsync().ConfigureAwait(false);
                    _db = null;
                }
            }
            finally { _mutex.Release(); }
        }

        public async Task PutSessionData(DateTime timestampUtc, string sessionType)
        {
            await EnsureDb();

            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await InTransactionAsync(async () =>
                {
                    switch (sessionType)
                    {
                        case "start":
                            {
                                var openId = await GetCurrentOpenSessionIdUnsafeAsync();
                                if (!string.IsNullOrWhiteSpace(openId))
                                {
                                    var closeSql = $@"
                                    UPDATE {SessionsTable}
                                    SET EndedAt = ?
                                    WHERE Id = ?;";
                                    await ExecRetryAsync(closeSql, timestampUtc, openId);
                                }

                                var insertStartSql = $@"
                                    INSERT INTO {SessionsTable} (Id, SessionId, StartedAt, EndedAt)
                                    VALUES (?, ?, ?, ?);";
                                await ExecRetryAsync(insertStartSql, Guid.NewGuid().ToString(), null, timestampUtc, null);
                                break;
                            }

                        case "end":
                            {
                                var openId = await GetCurrentOpenSessionIdUnsafeAsync();
                                if (!string.IsNullOrWhiteSpace(openId))
                                {
                                    var updateSql = $@"
                                    UPDATE {SessionsTable}
                                    SET EndedAt = ?
                                    WHERE Id = ?;";
                                    await ExecRetryAsync(updateSql, timestampUtc, openId);
                                }
                                else
                                {
                                    var insertEndSql = $@"
                                    INSERT INTO {SessionsTable} (Id, SessionId, StartedAt, EndedAt)
                                    VALUES (?, ?, ?, ?);";
                                    await ExecRetryAsync(insertEndSql, Guid.NewGuid().ToString(), null, null, timestampUtc);
                                }
                                break;
                            }

                        default:
                            Debug.WriteLine("The session type does not exist");
                            break;
                    }
                });
            }
            finally { _mutex.Release(); }
        }

        public async Task UpdateLogsWithCurrentSessionId()
        {
            await EnsureDb();

            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await InTransactionAsync(async () =>
                {
                    var selectSql = $@"
                        SELECT Id
                        FROM {SessionsTable}
                        WHERE EndedAt IS NULL
                        ORDER BY StartedAt DESC
                        LIMIT 1;";
                    var currentSessionId = await _db.ExecuteScalarAsync<string>(selectSql).ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(currentSessionId))
                    {
                        Debug.WriteLine("No open session found, logs not updated");
                        return;
                    }

                    var updateByRowIdSql = $@"
                                    UPDATE {LogsTable}
                                    SET SessionId = ?
                                    WHERE _rowid_ = (SELECT _rowid_ FROM {LogsTable} ORDER BY _rowid_ DESC LIMIT 1);";
                    await ExecRetryAsync(updateByRowIdSql, currentSessionId);

                    var changes = await GetLastChangeCountAsync().ConfigureAwait(false);
                    if (changes == 1) return;

                    var updateByIdSql = $@"
                                    UPDATE {LogsTable}
                                    SET SessionId = ?
                                    WHERE Id = (SELECT Id FROM {LogsTable} ORDER BY Id DESC LIMIT 1);";
                    await ExecRetryAsync(updateByIdSql, currentSessionId);

                    changes = await GetLastChangeCountAsync().ConfigureAwait(false);
                    if (changes == 0)
                    {
                        Debug.WriteLine("No logs to update (table empty?)");
                    }
                });
            }
            finally { _mutex.Release(); }
        }

        public async Task UpdateEventsWithCurrentSessionId()
        {
            await EnsureDb();

            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await InTransactionAsync(async () =>
                {
                    var selectSql = $@"
                        SELECT Id
                        FROM {SessionsTable}
                        WHERE EndedAt IS NULL
                        ORDER BY StartedAt DESC
                        LIMIT 1;";
                    var currentSessionId = await _db.ExecuteScalarAsync<string>(selectSql).ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(currentSessionId))
                    {
                        Debug.WriteLine("No open session found, logs not updated");
                        return;
                    }

                    var updateByRowIdSql = $@"
                                    UPDATE {EventsTable}
                                    SET SessionId = ?
                                    WHERE _rowid_ = (SELECT _rowid_ FROM {EventsTable} ORDER BY _rowid_ DESC LIMIT 1);";
                    await ExecRetryAsync(updateByRowIdSql, currentSessionId);

                    var changes = await GetLastChangeCountAsync().ConfigureAwait(false);
                    if (changes == 1) return;

                    var updateByIdSql = $@"
                                    UPDATE {EventsTable}
                                    SET SessionId = ?
                                    WHERE Id = (SELECT Id FROM {EventsTable} ORDER BY Id DESC LIMIT 1);";
                    await ExecRetryAsync(updateByIdSql, currentSessionId);

                    changes = await GetLastChangeCountAsync().ConfigureAwait(false);
                    if (changes == 0)
                    {
                        Debug.WriteLine("No logs to update (table empty?)");
                    }
                });
            }
            finally { _mutex.Release(); }
        }

        private async Task EnsureDb()
        {
            if (_db == null)
                throw new InvalidOperationException("Llama InitializeAsync() antes de usar StorableApp.");
            await Task.CompletedTask;
        }

        private async Task InTransactionAsync(Func<Task> body)
        {
            await ExecRetryAsync("BEGIN;");
            try
            {
                await body().ConfigureAwait(false);
                await ExecRetryAsync("COMMIT;");
            }
            catch
            {
                try { await ExecRetryAsync("ROLLBACK;"); } catch { }
                throw;
            }
        }

        public async Task<string?> GetCurrentOpenSessionIdUnsafeAsync()
        {
            try
            {
                var sql = $@"
                            SELECT Id
                            FROM {SessionsTable}
                            WHERE EndedAt IS NULL
                            ORDER BY StartedAt DESC
                            LIMIT 1;";
                var id = await _db.ExecuteScalarAsync<string>(sql).ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(id) ? null : id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting current open session ID: {ex.StackTrace}");
            }
            return null;
        }

        private async Task<int> GetLastChangeCountAsync()
        {
            return await _db.ExecuteScalarAsync<int>("SELECT changes();").ConfigureAwait(false);
        }

        private async Task ExecRetryAsync(string sql, params object[] args)
        {
            string lastMessage = "unknown";
            bool isPragma = sql.TrimStart().StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase);

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                try
                {
                    if (isPragma)
                    {
                        _ = await _db.ExecuteScalarAsync<long>(sql).ConfigureAwait(false);
                    }
                    else
                    {
                        await _db.ExecuteAsync(sql, args).ConfigureAwait(false);
                    }
                    return;
                }
                catch (SQLiteException ex) when (IsBusyOrLocked(ex))
                {
                    lastMessage = ex.Message ?? ex.Result.ToString();
                    await SleepWithBackoff(attempt).ConfigureAwait(false);
                    continue;
                }
                catch (SQLiteException ex)
                {
                    lastMessage = ex.Message ?? ex.Result.ToString();
                    throw new InvalidOperationException($"SQLite error [{ex.Result}] on: {sql}\n{lastMessage}", ex);
                }
            }
            throw new TimeoutException($"execRetry exhausted attempts for: {sql} (last: {lastMessage})");
        }

        private static bool IsBusyOrLocked(SQLiteException ex)
        {
            return ex.Result == SQLite3.Result.Busy || ex.Result == SQLite3.Result.Locked;
        }

        private static async Task SleepWithBackoff(int attempt)
        {
            var pow = Math.Min(attempt, 7);
            var delay = Math.Min(MaxDelayMs, BaseDelayMs * (1 << pow));
            var jitter = _rng.Next(0, 2501);
            await Task.Delay(delay + jitter).ConfigureAwait(false);
        }
    }
}
