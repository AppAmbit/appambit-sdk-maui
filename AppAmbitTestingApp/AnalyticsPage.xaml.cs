using Shared.Utils;
using AppAmbit;
using AppAmbit.Services;
using static AppAmbitTestingApp.FormattedRequestSize;
using static System.Linq.Enumerable;
using AppAmbit.Models.Analytics;
using AppAmbit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


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
        foreach (int index in Range(start: 0, count: 30))
        {
            var date = DateUtils.GetUtcNow.AddDays(-index);
            await Analytics.TrackEvent("30 Daily events", new Dictionary<string, string> { { "30 Daily events", "Event" } }, date);
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
        DateTime startDate = DateUtils.GetUtcNow.AddDays(-30);
        var offlineSessions = new List<SessionData>();


        foreach (var index in Range(1, 30))
        {
            DateTime dateStartSession = startDate.AddDays(index);
            dateStartSession = dateStartSession.Date.AddHours(random.Next(0, 23)).AddMinutes(random.Next(0, 59));

            offlineSessions.Add(new SessionData
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = null,
                Timestamp = dateStartSession,
                SessionType = SessionType.Start
            });

            var durationEnd = TimeSpan.FromMinutes(random.Next(1, 24 * 60));
            DateTime dateEndSession = dateStartSession.Add(durationEnd);

            offlineSessions.Add(new SessionData
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = null,
                Timestamp = dateEndSession,
                SessionType = SessionType.End
            });

        }

        var settings = new JsonSerializerSettings
        {
            Converters = [new StringEnumConverter()],
            Formatting = Formatting.Indented
        };
        
         var json = JsonConvert.SerializeObject(offlineSessions, settings);

        var filePath = Path.Combine(FileSystem.AppDataDirectory, OfflineSessionsFile);
        File.WriteAllText(filePath, json);

        await DisplayAlert("Info", $"Turn off and Turn on internet to send the sessions.", "Ok");
    }

}