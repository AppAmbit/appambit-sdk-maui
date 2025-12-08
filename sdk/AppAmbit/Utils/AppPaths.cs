using System;
using System.IO;
#if MACCATALYST || IOS
using Foundation;
#endif
#if WINDOWS
using System.Reflection;
#endif

namespace AppAmbit
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
#elif WINDOWS
                string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AppAmbit";
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var fullPath = Path.Combine(basePath, appName);
                Directory.CreateDirectory(fullPath);

                return fullPath;
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
#elif WINDOWS
            string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AppAmbit";
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(basePath, appName);
            Directory.CreateDirectory(appDir);

            return Path.Combine(appDir, databaseFileName);
#else
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(dir)) dir = AppContext.BaseDirectory;
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, databaseFileName);
#endif
        }
    }
}
