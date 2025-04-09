using AppAmbit;

namespace AppAmbitTestingApp;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        await Analytics.TrackEvent("ButtonClicked", new Dictionary<string, object> { { "Count", "41" }});
    }
    
    private async void Button_OnClickedTestEvent(object? sender, EventArgs e)
    {
        await Analytics.GenerateTestEvent();
    }
}