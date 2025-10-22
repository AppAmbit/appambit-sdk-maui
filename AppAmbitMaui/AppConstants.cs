using System;
using System.IO;

namespace AppAmbit;

internal class AppConstants
{
    private const string DatabaseFileName = "AppAmbit.db3";

    internal const SQLite.SQLiteOpenFlags Flags = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;

    internal static string DatabasePath
    {
        get
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(baseDir))
                baseDir = AppContext.BaseDirectory;
            Directory.CreateDirectory(baseDir);
            return Path.Combine(baseDir, DatabaseFileName);
        }
    }

    internal const string DidCrashFileName = "did_app_crash.json";
    
    internal const string UnknownFileName = nameof(UnknownFileName); 
    
    internal const string UnknownClass = nameof(UnknownClass);
    
    internal const string NoStackTraceAvailable = nameof(NoStackTraceAvailable);

    internal const int TrackEventNameMaxLimit = 80;

    internal const int TrackEventMaxPropertyLimit = 20;

    internal const int TrackEventPropertyMaxCharacters = 80;
}
