using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using SQLitePCL;
using Microsoft.Maui.Storage;

namespace AppAmbitTestingApp.Utils
{
    public sealed class StorableApp
    {
        private const string SessionsTable = "SessionEntity";
        private const string LogsTable     = "LogEntity";
        private const string EventsTable   = "EventEntity";

        private static readonly Lazy<StorableApp> _lazy = new(() => new StorableApp());
        public static StorableApp Shared => _lazy.Value;

        private static readonly string DatabasePath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "AppAmbit.db3");
        private const SQLiteOpenFlags PoolFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        private const bool StoreTicks = false;
        private readonly SemaphoreSlim _mutex = new(1, 1);

        private const int MaxAttempts = 12;
        private const int BaseDelayMs = 20;
        private const int MaxDelayMs  = 1200;
        private static readonly Random _rng = new();

        private StorableApp() { }
        
        public async Task PutSessionData(DateTime timestampUtc, string sessionType)
        {
            var tsUtc = DateTime.SpecifyKind(timestampUtc, DateTimeKind.Utc);
            var sessionIdNumeric = Guid.NewGuid();

            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await WithConnection(async db =>
                {
                    await BeginAsync(db).ConfigureAwait(false);
                    try
                    {
                        switch (sessionType)
                        {
                            case "start":
                                {
                                    var openId = await GetCurrentOpenSessionInternal(db).ConfigureAwait(false);
                                    if (!string.IsNullOrWhiteSpace(openId))
                                    {
                                        await ExecRetryAsync(db, $@"UPDATE {SessionsTable} SET EndedAt = ? WHERE Id = ?;",
                                            tsUtc, openId).ConfigureAwait(false);
                                    }

                                    await ExecRetryAsync(db, $@"
                                    INSERT INTO {SessionsTable} (Id, StartedAt, EndedAt)
                                    VALUES (?, ?, NULL);",
                                        Guid.NewGuid().ToString(), tsUtc).ConfigureAwait(false);
                                    break;
                                }

                            case "end":
                                {
                                    var updated = await db.ExecuteAsync($@"
                                    UPDATE {SessionsTable}
                                    SET EndedAt = ?
                                    WHERE EndedAt IS NULL;", tsUtc).ConfigureAwait(false);

                                    if (updated == 0)
                                    {
                                        await ExecRetryAsync(db, $@"
                                        INSERT INTO {SessionsTable} (Id, SessionId, StartedAt, EndedAt)
                                        VALUES (?, NULL, ?);",
                                            Guid.NewGuid().ToString(), tsUtc).ConfigureAwait(false);
                                    }
                                    break;
                                }

                            default:
                                Debug.WriteLine("Unknown session type");
                                break;
                        }

                        await CommitAsync(db).ConfigureAwait(false);
                    }
                    catch
                    {
                        await RollbackSafeAsync(db).ConfigureAwait(false);
                        throw;
                    }
                }).ConfigureAwait(false);
            }
            finally { _mutex.Release(); }
        }

        public async Task UpdateLogsWithCurrentSessionId()
        {
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await WithConnection(async db =>
                {
                    await BeginAsync(db).ConfigureAwait(false);
                    try
                    {
                        var currentSessionId = await db.ExecuteScalarAsync<string>($@"
                            SELECT SessionId
                            FROM {SessionsTable}
                            WHERE EndedAt IS NULL
                            ORDER BY StartedAt DESC
                            LIMIT 1;").ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(currentSessionId))
                        {
                            Debug.WriteLine("No open session found, logs not updated");
                            await RollbackSafeAsync(db).ConfigureAwait(false);
                            return;
                        }

                        await ExecRetryAsync(db, $@"
                            UPDATE {LogsTable}
                            SET SessionId = ?
                            WHERE _rowid_ = (SELECT _rowid_ FROM {LogsTable} ORDER BY _rowid_ DESC LIMIT 1);",
                            currentSessionId).ConfigureAwait(false);

                        await ExecRetryAsync(db, $@"
                            UPDATE {LogsTable}
                            SET SessionId = ?
                            WHERE SessionId IS NULL
                               OR TRIM(SessionId) = ''
                               OR NOT (SessionId GLOB '[0-9]*');",
                            currentSessionId).ConfigureAwait(false);

                        await CommitAsync(db).ConfigureAwait(false);
                    }
                    catch
                    {
                        await RollbackSafeAsync(db).ConfigureAwait(false);
                        throw;
                    }
                }).ConfigureAwait(false);
            }
            finally { _mutex.Release(); }
        }

        public async Task UpdateEventsWithCurrentSessionId()
        {
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                await WithConnection(async db =>
                {
                    await BeginAsync(db).ConfigureAwait(false);
                    try
                    {
                        var currentSessionId = await db.ExecuteScalarAsync<string>($@"
                            SELECT SessionId
                            FROM {SessionsTable}
                            WHERE EndedAt IS NULL
                            ORDER BY StartedAt DESC
                            LIMIT 1;").ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(currentSessionId))
                        {
                            Debug.WriteLine("No open session found, events not updated");
                            await RollbackSafeAsync(db).ConfigureAwait(false);
                            return;
                        }

                        await ExecRetryAsync(db, $@"
                            UPDATE {EventsTable}
                            SET SessionId = ?
                            WHERE _rowid_ = (SELECT _rowid_ FROM {EventsTable} ORDER BY _rowid_ DESC LIMIT 1);",
                            currentSessionId).ConfigureAwait(false);

                        await ExecRetryAsync(db, $@"
                            UPDATE {EventsTable}
                            SET SessionId = ?
                            WHERE SessionId IS NULL
                               OR TRIM(SessionId) = ''
                               OR NOT (SessionId GLOB '[0-9]*');",
                            currentSessionId).ConfigureAwait(false);

                        await CommitAsync(db).ConfigureAwait(false);
                    }
                    catch
                    {
                        await RollbackSafeAsync(db).ConfigureAwait(false);
                        throw;
                    }
                }).ConfigureAwait(false);
            }
            finally { _mutex.Release(); }
        }

        public async Task<string?> GetCurrentOpenSession()
        {
            return await WithConnection(async db =>
            {
                try
                {
                    var id = await db.ExecuteScalarAsync<string>($@"
                        SELECT Id
                        FROM {SessionsTable}
                        WHERE EndedAt IS NULL
                        ORDER BY StartedAt DESC
                        LIMIT 1;").ConfigureAwait(false);
                    return string.IsNullOrWhiteSpace(id) ? null : id;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting current open session row Id: {ex.Message}");
                    return null;
                }
            }).ConfigureAwait(false);
        }

        private static SQLiteAsyncConnection CreateConnection()
            => new SQLiteAsyncConnection(DatabasePath, PoolFlags, storeDateTimeAsTicks: StoreTicks);

        private static async Task WithConnection(Func<SQLiteAsyncConnection, Task> body)
        {
            var db = CreateConnection();
            try { await body(db).ConfigureAwait(false); }
            finally { await SafeCloseAsync(db).ConfigureAwait(false); }
        }

        private static async Task<T> WithConnection<T>(Func<SQLiteAsyncConnection, Task<T>> body)
        {
            var db = CreateConnection();
            try { return await body(db).ConfigureAwait(false); }
            finally { await SafeCloseAsync(db).ConfigureAwait(false); }
        }

        private static async Task SafeCloseAsync(SQLiteAsyncConnection db)
        {
            try { await db.CloseAsync().ConfigureAwait(false); } catch { /* swallow */ }
        }

        private static async Task BeginAsync(SQLiteAsyncConnection db) => await ExecRetryAsync(db, "BEGIN;").ConfigureAwait(false);
       
        private static async Task CommitAsync(SQLiteAsyncConnection db) => await ExecRetryAsync(db, "COMMIT;").ConfigureAwait(false);
    
        private static async Task RollbackSafeAsync(SQLiteAsyncConnection db)
        {
            try { await ExecRetryAsync(db, "ROLLBACK;").ConfigureAwait(false); } catch { /* swallow */ }
        }

        private static async Task<string?> GetCurrentOpenSessionInternal(SQLiteAsyncConnection db)
        {
            var id = await db.ExecuteScalarAsync<string>($@"
                SELECT Id
                FROM {SessionsTable}
                WHERE EndedAt IS NULL
                ORDER BY StartedAt DESC
                LIMIT 1;").ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        private static async Task ExecRetryAsync(SQLiteAsyncConnection db, string sql, params object[] args)
        {
            string lastMessage = "unknown";
            bool isPragma = sql.TrimStart().StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase);

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                try
                {
                    if (isPragma)
                        _ = await db.ExecuteScalarAsync<long>(sql).ConfigureAwait(false);
                    else
                        await db.ExecuteAsync(sql, args).ConfigureAwait(false);
                    return;
                }
                catch (SQLiteException ex) when (ex.Result == SQLite3.Result.Busy || ex.Result == SQLite3.Result.Locked)
                {
                    var pow   = Math.Min(attempt, 7);
                    var delay = Math.Min(MaxDelayMs, BaseDelayMs * (1 << pow));
                    var jitter = _rng.Next(0, 2501);
                    await Task.Delay(delay + jitter).ConfigureAwait(false);
                    lastMessage = ex.Message ?? ex.Result.ToString();
                }
                catch (SQLiteException ex)
                {
                    lastMessage = ex.Message ?? ex.Result.ToString();
                    throw new InvalidOperationException($"SQLite error [{ex.Result}] on: {sql}\n{lastMessage}", ex);
                }
            }
            throw new TimeoutException($"execRetry exhausted attempts for: {sql} (last: {lastMessage})");
        }
    }
}
