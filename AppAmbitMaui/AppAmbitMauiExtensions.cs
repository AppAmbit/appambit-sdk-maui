using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;

namespace AppAmbit;

public static class AppAmbitMauiExtensions
{
    private static string? _lastPageName;
    private static bool _firstNavSkipped;
    private static Shell? _hookedShell;
    private static NavigationPage? _hookedNavPage;

    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder, string appKey)
    {
        AppAmbitSdk.MarkConfiguredByBuilder();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, state) => { AppAmbitSdk.InternalStart(appKey); HookPageNavigation(); });
                android.OnPause(activity => { AppAmbitSdk.InternalSleep(); });
                android.OnResume(activity => { _ = AppAmbitSdk.InternalResume(); HookPageNavigation(); });
                android.OnStop(activity => { AppAmbitSdk.InternalSleep(); });
                android.OnRestart(activity => { _ = AppAmbitSdk.InternalResume(); HookPageNavigation(); });
                android.OnDestroy(activity => { AppAmbitSdk.InternalEnd(); });
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((application, options) =>
                {
                    AppAmbitSdk.InternalStart(appKey);
                    HookPageNavigation();
                    return true;
                });
                ios.DidEnterBackground(application => { AppAmbitSdk.InternalSleep(); });
                ios.WillEnterForeground(application => { _ = AppAmbitSdk.InternalResume(); HookPageNavigation(); });
                ios.WillTerminate(application => { AppAmbitSdk.InternalEnd(); });
            });
#endif
        });

        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();

        return builder;
    }

    private static async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        Debug.WriteLine("OnConnectivityChanged");
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            BreadcrumbManager.SaveFile(BreadcrumbsConstants.offline);
            return;
        }

        if (!AppAmbitSdk.InternalTokenIsValid())
            await AppAmbitSdk.InternalEnsureToken(null);

        BreadcrumbManager.LoadBreadcrumbsFromFile();
        await SessionManager.SendEndSessionFromDatabase();
        await SessionManager.SendStartSessionIfExist();
        await Crashes.LoadCrashFileIfExists();        
        await BreadcrumbManager.AddAsync(BreadcrumbsConstants.online);
        await AppAmbitSdk.InternalSendPending();
    }

    private static void HookPageNavigation()
    {
        var app = Application.Current;
        if (app == null) return;

        app.PropertyChanged -= OnAppPropertyChanged;
        app.PropertyChanged += OnAppPropertyChanged;

        AttachToMainPage(app.MainPage);
    }

    private static void OnAppPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Application.MainPage))
        {
            var app = sender as Application;
            AttachToMainPage(app?.MainPage);
        }
    }

    private static void AttachToMainPage(Page? page)
    {
        if (_hookedShell != null)
        {
            _hookedShell.Navigated -= OnShellNavigated;
            _hookedShell = null;
        }
        if (_hookedNavPage != null)
        {
            _hookedNavPage.Pushed -= OnNavPushed;
            _hookedNavPage.Popped -= OnNavPopped;
            _hookedNavPage = null;
        }

        if (page is Shell shell)
        {
            _hookedShell = shell;
            shell.Navigated += OnShellNavigated;
            _lastPageName = GetDisplayName(shell.CurrentPage);
            _firstNavSkipped = false;
            return;
        }

        if (page is NavigationPage nav)
        {
            _hookedNavPage = nav;
            nav.Pushed += OnNavPushed;
            nav.Popped += OnNavPopped;
            _lastPageName = GetDisplayName(nav.CurrentPage);
            _firstNavSkipped = false;
        }
        else
        {
            _lastPageName = GetDisplayName(page);
            _firstNavSkipped = false;
        }
    }

    private static async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var shell = sender as Shell;
        var currentName = GetDisplayName(shell?.CurrentPage);
        if (string.IsNullOrWhiteSpace(currentName)) return;

        if (!_firstNavSkipped)
        {
            _firstNavSkipped = true;
            _lastPageName = currentName;
            return;
        }

        var prev = _lastPageName;
        if (prev == null || prev == currentName)
        {
            _lastPageName = currentName;
            return;
        }

        await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onDisappear}: {prev}");
        _lastPageName = currentName;
        await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onAppear}: {currentName}");
    }

    private static async void OnNavPushed(object? sender, NavigationEventArgs e)
    {
        var newName = GetDisplayName(e?.Page);
        if (string.IsNullOrWhiteSpace(newName)) return;

        if (!_firstNavSkipped)
        {
            _firstNavSkipped = true;
            _lastPageName = newName;
            return;
        }

        var prev = _lastPageName;
        if (prev == newName)
        {
            _lastPageName = newName;
            return;
        }

        if (!string.IsNullOrWhiteSpace(prev))
            await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onDisappear}: {prev}");

        _lastPageName = newName;
        await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onAppear}: {newName}");
    }

    private static async void OnNavPopped(object? sender, NavigationEventArgs e)
    {
        var nav = sender as NavigationPage;
        var currentName = GetDisplayName(nav?.CurrentPage);
        if (string.IsNullOrWhiteSpace(currentName)) return;

        var prev = _lastPageName;
        if (prev == currentName) return;

        if (!string.IsNullOrWhiteSpace(prev))
            await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onDisappear}: {prev}");

        _lastPageName = currentName;
        await BreadcrumbManager.AddAsync($"{BreadcrumbsConstants.onAppear}: {currentName}");
    }

    private static string GetDisplayName(Page? p)
    {
        var t = p?.Title;
        if (!string.IsNullOrWhiteSpace(t)) return t.Trim();
        return p?.GetType().Name ?? string.Empty;
    }
}