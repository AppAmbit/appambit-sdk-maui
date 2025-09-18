using AppAmbit;
using AppAmbit.Services;
using static AppAmbitTestingApp.FormattedRequestSize;
using static System.Linq.Enumerable;
using AppAmbitTestingApp.Utils;


namespace AppAmbitTestingApp;

public partial class AnalyticsPage : ContentPage
{
    private const string OfflineSessionsFile = "OfflineSessions.json";

    public AnalyticsPage()
    {
        InitializeComponent();
    }

    private async void Button_OnStartSession(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();

        await Analytics.StartSession();

        ButtonStartSession.Padding = 10;
        ButtonStartSession.FontSize = 12;
        ButtonStartSession.Text = $"Start Session ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private void OnTestToken(object? sender, EventArgs eventArgs)
    {
        Analytics.ClearToken();
    }

    private async void OnTokenRefreshTest(object? sender, EventArgs eventArgs)
    {
        LoggingHandler.ResetTotalSize();
        Analytics.ClearToken();
        var logsTask = Range(0, 5).Select(
            _ => Task.Run(() =>
            {
                Crashes.LogError("Sending 5 errors after an invalid token");
            }));

        var eventsTask = Range(0, 5).Select(
            _ => Task.Run(() =>
            {
                Analytics.TrackEvent("Sending 5 events after an invalid token",
                new Dictionary<string, string>
                {{"Test Token", "5 events sent"}});
            }));
        await Task.WhenAll(logsTask);
        Analytics.ClearToken();
        await Task.WhenAll(eventsTask);
        await DisplayAlert("Info", "5 events and errors sent", "Ok");
        ButtonRefreshTest.Padding = 10;
        ButtonRefreshTest.FontSize = 12;
        ButtonRefreshTest.Text = $"Token refresh test ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void Button_OnEndSession(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();

        await Analytics.EndSession();

        ButtonEndSession.Padding = 10;
        ButtonEndSession.FontSize = 12;
        ButtonEndSession.Text = $"End Session ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();

        await Analytics.TrackEvent("ButtonClicked", new Dictionary<string, string> { { "Count", "41" } });

        ButtonClickedEventWProperty.Padding = 10;
        ButtonClickedEventWProperty.FontSize = 12;
        ButtonClickedEventWProperty.Text = $"Send 'Button Clicked' Event w/ property ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void Button_OnClickedTestEvent(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();
        await Analytics.GenerateTestEvent();

        ButtonDefaultClickedEventWProperty.Padding = 10;
        ButtonDefaultClickedEventWProperty.FontSize = 12;
        ButtonDefaultClickedEventWProperty.Text = $"Send Default Event w/ property ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void Button_OnClickedTestLimitsEvent(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();
        //300 characters:
        var _300Characters = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
        var _300Characters2 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678902";
        var properties = new Dictionary<string, string>
        {
            { _300Characters, _300Characters },
            { _300Characters2, _300Characters2 }
        };
        await Analytics.TrackEvent(_300Characters, properties);
        ButtonMax300LengthEvent.Padding = 10;
        ButtonMax300LengthEvent.FontSize = 12;
        ButtonMax300LengthEvent.Text = $"Send Max-300-Length Event ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void Button_OnClickedTestMaxPropertiesEvent(object? sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();

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
        ButtonMax20PropertiesEvent.Padding = 10;
        ButtonMax20PropertiesEvent.FontSize = 12;
        ButtonMax20PropertiesEvent.Text = $"Send Max-20-Properties Event ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void OnSend30DailyEvents(object? sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await DisplayAlert("Info", "Turn off internet and try again", "Ok");
            return;
        }

        await StorableApp.Shared.ClosePreviousSessionIfExists(DateTime.UtcNow);

        foreach (int index in Range(start: 0, count: 30))
        {
            var date = DateTime.UtcNow.AddDays(-index);
            await StorableApp.Shared.PutSessionData(date, "start");
            await Analytics.TrackEvent("30 Daily events", new Dictionary<string, string> { { "30 Daily events", "Event" } }, date);
            await StorableApp.Shared.UpdateEventsWithCurrentSessionId();
            await StorableApp.Shared.PutSessionData(date.AddSeconds(2), "end");
            await Task.Delay(500);
        }
        await DisplayAlert("Info", "Events generated, turn on internet", "Ok");
    }

    private async void OnGenerateBatchEvents(object? sender, EventArgs e)
    {
        await DisplayAlert("Info", "Turn off internet", "Ok");
        foreach (int index in Range(1, 220))
        {
            await Analytics.TrackEvent("Test Batch TrackEvent", new Dictionary<string, string> { { "test1", "test1" } });
        }
        await DisplayAlert("Info", "Events generated", "Ok");
        await DisplayAlert("Info", "Turn on internet to send the events", "Ok");
    }

    private async void OnGenerate30DaysTestSessions(object? sender, EventArgs e)
    {
        var random = new Random();
        DateTime startDate = DateTime.UtcNow.AddDays(-30);

        await StorableApp.Shared.ClosePreviousSessionIfExists(DateTime.UtcNow);
        foreach (var index in Range(1, 30))
        {
            DateTime dateStartSession = startDate.AddDays(index);
            dateStartSession = dateStartSession.Date
                .AddHours(random.Next(0, 24))
                .AddMinutes(random.Next(0, 60));

            await StorableApp.Shared.PutSessionData(dateStartSession, "start");

            var durationEnd = TimeSpan.FromMinutes(random.Next(1, 24 * 60));
            DateTime dateEndSession = dateStartSession.Add(durationEnd);

            await StorableApp.Shared.PutSessionData(dateEndSession, "end");
        }

        await DisplayAlert("Info", "30-day sessions generated in DB.", "Ok");
    }
}