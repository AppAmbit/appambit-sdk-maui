#if AVALONIA
using System;
using System.Net.NetworkInformation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
namespace AppAmbit.Avalonia;
/// <summary>
/// Avalonia desktop lifecycle hook for macOS (AppKit) heads.
/// Mirrors AppAmbitMacOs intent without depending on Catalyst.
/// </summary>
internal static class AvaloniaMacOs
{
    private static bool _initialized;
    private static bool _started;
    private static Window? _mainWindow;
    public static void Register(IClassicDesktopStyleApplicationLifetime lifetime, string appKey)
    {
        if (_initialized) return;
        _initialized = true;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        lifetime.Exit += OnExit;
        lifetime.ShutdownRequested += OnShutdownRequested;
        lifetime.Startup += (_, __) => AttachWindowEvents(lifetime.MainWindow);
        AttachWindowEvents(lifetime.MainWindow);
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        StartIfNeeded(appKey);
        _ = AppAmbitSdk.InternalResume();
    }
    private static void AttachWindowEvents(Window? window)
    {
        if (window == null || ReferenceEquals(_mainWindow, window))
            return;
        _mainWindow = window;
        window.Opened += OnWindowOpened;
        window.Closed += OnWindowClosed;
        window.Activated += OnWindowActivated;
        window.Deactivated += OnWindowDeactivated;
    }
    private static void StartIfNeeded(string appKey)
    {
        if (_started) return;
        _started = true;
        AppAmbitSdk.InternalStart(appKey);
    }
    private static async void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        try
        {
            var isOnline = e.IsAvailable;
            await BreadcrumbManager.AddAsync(isOnline ? BreadcrumbsConstants.online : BreadcrumbsConstants.offline);
            if (isOnline && !AppAmbitSdk.InternalTokenIsValid())
                await AppAmbitSdk.InternalEnsureToken(null);
        }
        catch
        {
            // swallow — breadcrumb/logging already best-effort
        }
    }
    private static void OnWindowOpened(object? sender, EventArgs e)
    {
        _ = AppAmbitSdk.InternalResume();
    }
    private static void OnWindowClosed(object? sender, EventArgs e)
    {
        AppAmbitSdk.InternalEnd();
    }
    private static void OnWindowActivated(object? sender, EventArgs e)
    {
        _ = AppAmbitSdk.InternalResume();
    }
    private static void OnWindowDeactivated(object? sender, EventArgs e)
    {
        AppAmbitSdk.InternalSleep();
    }
    private static void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        AppAmbitSdk.InternalEnd();
    }
    private static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        AppAmbitSdk.InternalEnd();
    }
    private static void OnProcessExit(object? sender, EventArgs e)
    {
        AppAmbitSdk.InternalEnd();
    }
}
#endif