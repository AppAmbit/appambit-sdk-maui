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
        ButtonStartSession.Padding = 10;
        ButtonStartSession.FontSize = 12;
        ButtonStartSession.Text = $"Start Session ({Analytics.FormattedSize(Analytics.GetRequestSize())})";
    }
    
    private async void Button_OnEndSession(object? sender, EventArgs e)
    {
        await Analytics.EndSession();
        ButtonEndSession.Padding = 10;
        ButtonEndSession.FontSize = 12;
        ButtonEndSession.Text = $"End Session ({Analytics.FormattedSize(Analytics.GetRequestSize())})";
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        await Analytics.TrackEvent("ButtonClicked", new Dictionary<string, string> { { "Count", "41" }});
        ButtonClickedEventWProperty.Padding = 10;
        ButtonClickedEventWProperty.FontSize = 12;
        ButtonClickedEventWProperty.Text = $"Send 'Button Clicked' Event w/ property {Analytics.FormattedSize(Analytics.GetRequestSize())}";
    }
    
    private async void Button_OnClickedTestEvent(object? sender, EventArgs e)
    {
        await Analytics.GenerateTestEvent();
        ButtonDefaultClickedEventWProperty.Padding = 10;
        ButtonDefaultClickedEventWProperty.FontSize = 12;
        ButtonDefaultClickedEventWProperty.Text = $"Send Default Event w/ property ({Analytics.FormattedSize(Analytics.GetRequestSize())})";
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
        ButtonMax300LengthEvent.Padding = 10;
        ButtonMax300LengthEvent.FontSize = 12;
        ButtonMax300LengthEvent.Text = $"Send Max-300-Length Event ({Analytics.FormattedSize(Analytics.GetRequestSize())})";
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
        ButtonMax20PropertiesEvent.Padding = 10;
        ButtonMax20PropertiesEvent.FontSize = 12;
        ButtonMax20PropertiesEvent.Text = $"Send Max-20-Properties Event ({Analytics.FormattedSize(Analytics.GetRequestSize())})";
    }

    private async void OnGenerateBatchEvents(object? sender, EventArgs e)
    {
        await DisplayAlert("Info", "Turn off internet", "Ok");
        double totalSize = 0;
        foreach (int index in Range( 1, 220 ))
        {
            await Analytics.TrackEvent("Test Batch TrackEvent",new Dictionary<string, string> { { "test1", "test1" } });
            totalSize += Analytics.GetRequestSize();
        }
        await DisplayAlert("Info", "Events generated", "Ok");
        await DisplayAlert("Info", "Turn on internet to send the events", "Ok");
        Button220BatchEvents.Padding = 10;
        Button220BatchEvents.FontSize = 12;
        Button220BatchEvents.Text = $"Send Batch of 220 Events ({Analytics.FormattedSize(totalSize)})";
    }
}