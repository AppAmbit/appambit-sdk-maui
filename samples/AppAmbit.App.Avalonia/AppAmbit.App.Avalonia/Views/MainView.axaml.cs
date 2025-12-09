using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using AppAmbit;
using System.Threading.Tasks;
using System.Linq;

namespace AppAmbitTestingAppAvalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        try
        {
            txtChangeUserId.Text = System.Guid.NewGuid().ToString();
            txtChangeUserEmail.Text = "test@gmail.com";
            txtCustomLogError.Text = "Test Log Message";
        }
        catch { }
    }

    private void OnNavCrashesClicked(object? sender, RoutedEventArgs e)
    {
        CrashesPanel.IsVisible = true;
        AnalyticsPanel.IsVisible = false;
    }

    private void OnNavAnalyticsClicked(object? sender, RoutedEventArgs e)
    {
        CrashesPanel.IsVisible = false;
        AnalyticsPanel.IsVisible = true;
    }

    private async void OnDidCrashClicked(object? sender, RoutedEventArgs e)
    {
        await Crashes.DidCrashInLastSession();
    }

    private async void OnChangeUserIdClicked(object? sender, RoutedEventArgs e)
    {
        var text = txtChangeUserId?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
        {
            Analytics.SetUserId(text);
        }
    }

    private async void OnChangeUserEmailClicked(object? sender, RoutedEventArgs e)
    {
        var text = txtChangeUserEmail?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "test@gmail.com";
            Analytics.SetUserEmail(text);
        }
    }

    private async void OnCustomLogErrorClicked(object? sender, RoutedEventArgs e)
    {
        var text = txtCustomLogError?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            Crashes.LogError(text);
        }
    }

    private async void OnDefaultLogErrorClicked(object? sender, RoutedEventArgs e)
    {
        Crashes.LogError("Test Log Error", new Dictionary<string, string>() { { "user_id", "1" } });
    }

    private async void OnSendExceptionLogErrorClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            throw new InvalidOperationException("This is a test exception for LogError.");
        }
        catch (Exception ex)
        {
            Crashes.LogError(ex);
        }
    }

    private async void OnThrowNewCrashClicked(object? sender, RoutedEventArgs e)
    {
        throw new NullReferenceException();
    }

    private async void OnGenerateCrashClicked(object? sender, RoutedEventArgs e)
    {
        await Crashes.GenerateTestCrash();
    }

    private async void OnSessionStartClicked(object? sender, RoutedEventArgs e)
    {
        await Analytics.StartSession();
    }

    private async void OnSessionEndClicked(object? sender, RoutedEventArgs e)
    {
        await Analytics.EndSession();
    }

    private async void OnInvalidateTokenClicked(object? sender, RoutedEventArgs e)
    {
        Analytics.ClearToken();
    }

    private async void OnTokenRefreshTestClicked(object? sender, RoutedEventArgs e)
    {
        Analytics.ClearToken();
        var logsTask = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
        {
            await Crashes.LogError("Sending 5 errors after an invalid token");
        }));

        var eventsTask = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
        {
            await Analytics.TrackEvent("Sending 5 events after an invalid token",
                new Dictionary<string, string>
                {{"Test Token", "5 events sent"}});
        }));
        await Task.WhenAll(logsTask);
        Analytics.ClearToken();
        await Task.WhenAll(eventsTask);
    }

    private async void OnSendButtonClickedClicked(object? sender, RoutedEventArgs e)
    {
        await Analytics.TrackEvent("ButtonClicked", new Dictionary<string, string> { { "Count", "41" } });
    }

    private async void OnSendDefaultEventClicked(object? sender, RoutedEventArgs e)
    {
        await Analytics.GenerateTestEvent();
    }

    private async void OnSendMax300LengthEventClicked(object? sender, RoutedEventArgs e)
    {
        //300 characters:
        var _300Characters = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
        var _300Characters2 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678902";
        var properties = new Dictionary<string, string>
        {
            { _300Characters, _300Characters },
            { _300Characters2, _300Characters2 }
        };
        await Analytics.TrackEvent(_300Characters, properties);
    }

    private async void OnSendMax20PropertiesEventClicked(object? sender, RoutedEventArgs e)
    {
        var properties = new Dictionary<string, string>
        {
            { "01", "01"},
            { "02", "02"},
            { "03", "03"},
            { "04", "04"},
            { "05", "05"},
            { "06", "06"},
            { "07", "07"},
            { "08", "08"},
            { "09", "09"},
            { "10", "10"},
            { "11", "11"},
            { "12", "12"},
            { "13", "13"},
            { "14", "14"},
            { "15", "15"},
            { "16", "16"},
            { "17", "17"},
            { "18", "18"},
            { "19", "19"},
            { "20", "20"},
            { "21", "21"},
            { "22", "22"},
            { "23", "23"},
            { "24", "24"},
            { "25", "25"},//25
        };
        await Analytics.TrackEvent("TestMaxProperties", properties);
    }

    private async void OnSend220EventsClicked(object? sender, RoutedEventArgs e)
    {
        foreach (int _ in Enumerable.Range(1, 220))
        {
            await Analytics.TrackEvent("Test Batch TrackEvent", new Dictionary<string, string> { { "test1", "test1" } });
        }
    }

    private async void OnChangeSecondActivityClicked(object? sender, RoutedEventArgs e)
    {
        
    }
}