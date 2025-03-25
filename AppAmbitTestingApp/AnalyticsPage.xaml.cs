using AppAmbit;

namespace AppAmbitTestingApp;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        Analytics.TrackEvent("ButtonClicked", new Dictionary<string, string> { { "Count", "41" }});
    }
    
    private void Button_OnClickedTestEvent(object? sender, EventArgs e)
    {
        Analytics.GenerateTestEvent();
    }
}