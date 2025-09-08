using AppAmbit;
using System.Diagnostics;

namespace AppAmbitTestingApp;

public partial class LoadPage : ContentPage
{
    public LoadPage()
    {
        InitializeComponent();
        this.BindingContext = this;
    }

    private string _title = "Test Message";

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    private async void OnSend500Events(object sender, EventArgs e)
    {
        bool isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        eventsLabel.IsVisible = true;
        for (int i = 0; i < 500; i++)
        {
            await Analytics.TrackEvent(_title, new Dictionary<string, string> { { "Test500", "Events" } });
            Debug.WriteLine($"Request Event: {i+1}");
            eventsLabel.Text = $"Sending event: {i+1} of 500";
            if (isOnline) {
                await Task.Delay(500);
            }
        }
        eventsLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Events generated", "Ok");
    }

    private async void OnSend500Logs(object sender, EventArgs e)
    {
        bool isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        logsLabel.IsVisible = true;
        for(int i = 0; i < 500; i++)
        {
            await Crashes.LogError(_title);
            Debug.WriteLine($"Request Log: {i+1}");
            logsLabel.Text = $"Sending Error: {i+1} of 500";
            if (isOnline)
            {
                await Task.Delay(500);
            }
        }
        logsLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Logs generated", "Ok");
    }

    private async void OnSend500Sessions(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.None)
        {
            await DisplayAlert("Info", "Turn off internet and try again", "Ok");
            return;
        }
        sessionsLabel.IsVisible = true;
        for (int i = 0; i < 500; i++)
        {
            await Analytics.StartSession();
            Analytics.ValidateOrInvaliteSession(true);
            await Analytics.EndSession();
            await Task.Delay(1000);
            sessionsLabel.Text = $"Sending Session: {i+1} of 500";
            await Task.Delay(1000);
        }
        sessionsLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Sessions requested", "Ok");
    }
}