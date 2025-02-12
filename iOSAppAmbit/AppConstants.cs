namespace iOSAppAmbit;

internal class AppConstants
{
    private const string DatabaseFileName = "AppAbmit.db3";

    internal const SQLite.SQLiteOpenFlags Flags = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;

    internal static string DatabasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DatabaseFileName);
}