using Shared.Utils;
using AppAmbit;
using static System.Linq.Enumerable;

namespace AppAmbitTestingApp;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }

    private async void Button_OnStartSession(object? sender, EventArgs e)
    {
        await Analytics.StartSession();
    }
    
    private async void Button_OnEndSession(object? sender, EventArgs e)
    {
        await Analytics.EndSession();
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        await Analytics.TrackEvent("ButtonClicked", new Dictionary<string, string> { { "Count", "41" }});
    }
    
    private async void Button_OnClickedTestEvent(object? sender, EventArgs e)
    {
        await Analytics.GenerateTestEvent();
    }

    private async void Button_OnClickedTestLimitsEvent(object? sender, EventArgs e)
    {
        //300 characters:
        var _300Characters ="123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
        var _300Characters2 ="1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678902";
        var properties = new Dictionary<string, string>
        {
            { _300Characters, _300Characters },
            { _300Characters2, _300Characters2 }
        };
        await Analytics.TrackEvent(_300Characters,properties );
    }

    private async void Button_OnClickedTestMaxPropertiesEvent(object? sender, EventArgs e)
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
        await Analytics.TrackEvent("TestMaxProperties",properties );
    }

    private async void OnSend30DailyEvents(object? sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await DisplayAlert("Info", "Turn off internet and try again", "Ok");
        }
        foreach(int index in Range(start: 1, count: 30))
        {
            var date = DateUtils.GetUtcNow.AddDays(-(30 - index));
            await Analytics.TrackEvent("30 Daily events", new Dictionary<string, string> { { "30 Daily events", "Event" } }, date);
        }
        await DisplayAlert("Info", "Events generated, turn on internet", "Ok");
    }

    private async void OnGenerateBatchEvents(object? sender, EventArgs e)
    {
        await DisplayAlert("Info", "Turn off internet", "Ok");
        foreach (int index in Range( 1, 220 ))
        {
            await Analytics.TrackEvent("Test Batch TrackEvent",new Dictionary<string, string> { { "test1", "test1" } });
        }
        await DisplayAlert("Info", "Events generated", "Ok");
        await DisplayAlert("Info", "Turn on internet to send the events", "Ok");
    }
}