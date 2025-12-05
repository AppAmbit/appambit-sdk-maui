using System;
using System.IO;
#if MACCATALYST || IOS
using Foundation;
#endif

namespace AppAmbitSdkCore
{
    public static class AppPaths
    {
        public static string AppDataDir
        {
            get
            {
#if MACCATALYST || IOS
                var baseDir = NSFileManager.DefaultManager
                    .GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User)[0].Path;
                var dir = Path.Combine(baseDir, NSBundle.MainBundle.BundleIdentifier ?? "AppAmbit");
                Directory.CreateDirectory(dir);
                return dir;
#else
                var p = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(p))
                    p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return p;
#endif
            }
        }

        public static string GetDatabaseFilePath(string databaseFileName)
        {
#if MACCATALYST || IOS
            var dir = Path.Combine(
                NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User)[0].Path,
                NSBundle.MainBundle.BundleIdentifier ?? "AppAmbit"
            );
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, databaseFileName);
#else
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(dir)) dir = AppContext.BaseDirectory;
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, databaseFileName);
#endif
        }
    }
}
