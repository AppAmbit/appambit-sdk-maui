using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace AppAmbit
{
    public static class AppAmbitWindows
    {
        private static string? _appKey;
        private static bool _initialized = false;
        public static void Start(string appKey)
        {
            if (_initialized) return;

            _appKey = appKey;
            _initialized = true;

            AppDomain.CurrentDomain.ProcessExit += OnAppExit;

            CheckNetworkStatus();

            OnStartApp(_appKey);
        }

        private static void OnStartApp(string appKey)
        {
            AppAmbitSdk.InternalStart(appKey);
        }

        private static void OnAppExit(object? sender, EventArgs e)
        {
            AppAmbitSdk.InternalEnd();
        }

        public static void AttachWindowEvents(Window window)
        {
            window.Activated += (_, _) =>
            {
                Log("(OnResume)");
            };

            window.Deactivated += (_, _) =>
            {
                Log("(OnPause)");
            };
        }

        private static void CheckNetworkStatus()
        {
            bool isOnline = NetworkInterface.GetIsNetworkAvailable();
            Log(isOnline ? "Online" : "Offline");
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
