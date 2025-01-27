using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAmbitTestingApp;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        AppAmbit.Analytics.TrackEventAsync("ButtonClicked", new Dictionary<string, string> { { "Count", "41" }});
    }
}