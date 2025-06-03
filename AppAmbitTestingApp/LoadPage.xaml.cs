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
        eventsLabel.IsVisible = true;
        for (int i = 0; i < 500; i++)
        {
            await Analytics.TrackEvent(_title, new Dictionary<string, string> { { "Test500", "Events" } });
            Debug.WriteLine($"Request Event: {i+1}");
            eventsLabel.Text = $"Sending event: {i+1} of 500";
        }
        eventsLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Events generated", "Ok");
    }

    private async void OnSend500Logs(object sender, EventArgs e)
    {
        logsLabel.IsVisible = true;
        for(int i = 0; i < 500; i++)
        {
            await Crashes.LogError(_title);
            Debug.WriteLine($"Request Log: {i+1}");
            logsLabel.Text = $"Sending Error: {i+1} of 500";
        }
        logsLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Logs generated", "Ok");
    }

    private async void OnSend500StartSession(object sender, EventArgs e)
    {
        startSessionLabel.IsVisible = true;
        for(int i = 0; i < 500; i++)
        {
            Analytics.ValidateOrInvaliteSession(false);
            await Analytics.StartSession();
            Debug.WriteLine($"Request StartSession: {i+1}");
            startSessionLabel.Text = $"Sending StartSession: {i+1} of 500";
        }
        startSessionLabel.IsVisible = false;
        await DisplayAlert("Info", "500 StartSessions requested", "Ok");
    }

    private async void OnSend500EndSession(object sender, EventArgs e)
    {
        endSessionLabel.IsVisible = true;
        for (int i = 0; i < 500; i++)
        {
            Analytics.ValidateOrInvaliteSession(true);
            await Analytics.EndSession();
            Debug.WriteLine($"Request EndSession: {i+1}");
            endSessionLabel.Text = $"Sending EndSession: {i+1} of 500";
        }
        endSessionLabel.IsVisible = false;
        await DisplayAlert("Info", "500 EndSessions requested", "Ok");
    }
    private async void OnSend500Tokens(object sender, EventArgs e)
    {
        tokenLabel.IsVisible = true;
        for (int i = 0; i < 500; i++)
        {
            await Analytics.RequestToken();
            Debug.WriteLine($"Request Token: {i+1}");
            tokenLabel.Text = $"Sending Token: {i+1} of 500";
        }
        tokenLabel.IsVisible = false;
        await DisplayAlert("Info", "500 Tokens requested", "Ok");
    }
}