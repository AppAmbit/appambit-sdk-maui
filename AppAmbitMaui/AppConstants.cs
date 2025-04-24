namespace AppAmbit;

internal class AppConstants
{
    private const string DatabaseFileName = "AppAbmit.db3";

    internal const SQLite.SQLiteOpenFlags Flags = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create |
                                                  SQLite.SQLiteOpenFlags.SharedCache;

    internal static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

    internal const string UnknownFileName = nameof(UnknownFileName);

    internal const string UnknownClass = nameof(UnknownClass);

    internal const string NoStackTraceAvailable = nameof(NoStackTraceAvailable);

    internal const int TrackEventNameMaxLimit = 80;

    internal const int TrackEventMaxPropertyLimit = 20;

    internal const int TrackEventPropertyMaxCharacters = 80;
}