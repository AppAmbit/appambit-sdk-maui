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
        for (int i = 0; i < 500; i++)
        {
            await Analytics.TrackEvent(_title, new Dictionary<string, string> { { "Test500", "Events" } });
            Debug.WriteLine($"Request Event: {i}");
            await Task.Delay(100);
        }
        await DisplayAlert("Info", "500 Events generated", "Ok");
    }

    private async void OnSend500Logs(object sender, EventArgs e)
    {
        for(int i = 0; i < 500; i++)
        {
            await Crashes.LogError(_title);
            Debug.WriteLine($"Request Log: {i}");
            await Task.Delay(300);
        }
        await DisplayAlert("Info", "500 Logs generated", "Ok");
    }

    private async void OnSend500StartSession(object sender, EventArgs e)
    {
        for(int i = 0; i < 500; i++)
        {
            await Analytics.StartSession();
            Debug.WriteLine($"Request StartSession: {i}");
            await Task.Delay(300);
        }
        await DisplayAlert("Info", "500 StartSessions requested", "Ok");
    }

    private async void OnSend500EndSession(object sender, EventArgs e)
    {
        for (int i = 0; i < 500; i++)
        {
            await Analytics.EndSession();
            Debug.WriteLine($"Request EndSession: {i}");
            await Task.Delay(300);
        }
        await DisplayAlert("Info", "500 EndSessions requested", "Ok");
    }
    private async void OnSend500Tokens(object sender, EventArgs e)
    {
        for (int i = 0; i < 500; i++)
        {
            await Analytics.RequestToken();
            Debug.WriteLine($"Request Token: {i}");
            await Task.Delay(300);
        }
        await DisplayAlert("Info", "500 Tokens requested", "Ok");
    }
}