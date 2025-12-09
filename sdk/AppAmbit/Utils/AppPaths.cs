using System;
using System.IO;
using System.Reflection;
#if MACCATALYST || IOS
using Foundation;
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
                if (OperatingSystem.IsWindows())
                {
                    string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AppAmbit";
                    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var fullPath = Path.Combine(basePath, appName);
                    Directory.CreateDirectory(fullPath);
                    return fullPath;
                }
                if (OperatingSystem.IsMacOS())
                {
                    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (string.IsNullOrWhiteSpace(basePath))
                        basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var dir = Path.Combine(basePath, "AppAmbit");
                    Directory.CreateDirectory(dir);
                    return dir;
                }
                var otherBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(otherBase))
                    otherBase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var otherDir = Path.Combine(otherBase, "AppAmbit");
                Directory.CreateDirectory(otherDir);
                return otherDir;
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
            if (OperatingSystem.IsWindows())
            {
                string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AppAmbit";
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appDir = Path.Combine(basePath, appName);
                Directory.CreateDirectory(appDir);
                return Path.Combine(appDir, databaseFileName);
            }
            if (OperatingSystem.IsMacOS())
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(basePath))
                    basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dir = Path.Combine(basePath, "AppAmbit");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, databaseFileName);
            }
            var other = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(other))
                other = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var otherDir = Path.Combine(other, "AppAmbit");
            Directory.CreateDirectory(otherDir);
            return Path.Combine(otherDir, databaseFileName);
#endif
        }
    }
}
