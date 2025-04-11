using AppAmbit;

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
}