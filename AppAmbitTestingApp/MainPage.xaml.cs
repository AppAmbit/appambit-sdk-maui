using System.ComponentModel;
using AppAmbit;
using AppAmbit.Models.Logs;
using static System.Linq.Enumerable;

namespace AppAmbitTestingApp;

public partial class MainPage : ContentPage
{

    private string _logMessage = "Test Log Message";

    public string LogMessage
    {
        get => _logMessage;
        set
        {
            if (_logMessage != value)
            {
                _logMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private string _userId = "";

    public string UserId
    {
        get => _userId;
        set
        {
            if (_userId != value)
            {
                _userId = value;
                OnPropertyChanged();
            }
        }
    }

    private string _userEmail = "test@gmail.com";

    public string UserEmail
    {
        get => _userEmail;
        set
        {
            if (_userEmail != value)
            {
                _userEmail = value;
                OnPropertyChanged();
            }
        }
    }
    
    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = this;
        UserId = Guid.NewGuid().ToString();
    }
    
    private async void OnGenerateLogsForBatch(object? sender, EventArgs e)
    {
        await DisplayAlert("Info", "Turn off internet", "Ok");
        foreach (int index in Range( 1, 220 ))
        {
            await Crashes.LogError("Test Batch LogError");
        }
        await DisplayAlert("Info", "Logs generated", "Ok");
        await DisplayAlert("Info", "Turn on internet to send the logs", "Ok");
    }

    private async void OnChangeUserId(object? sender, EventArgs e)
    {
        Analytics.SetUserId(UserId);
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }

    private async void OnChangeUserEmail(object? sender, EventArgs e)
    {
        Analytics.SetUserEmail(UserEmail);
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }

    private async void OnSendTestLog(object sender, EventArgs e)
    {
        await Crashes.LogError("Test Log Error",new Dictionary<string,string>(){{"user_id","1"}});
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }

    private async void OnSendTestException(object sender, EventArgs e)
    {
        try
        {
            throw new NullReferenceException();
        }
        catch (Exception exception)
        {
            await Crashes.LogError(exception,new Dictionary<string,string>(){{"user_id","1"}});
            await DisplayAlert("Info", "LogError Sent", "Ok");
        }
    }

    private async void OnSendTestLogWithClassFQN(object sender, EventArgs e)
    {
        await Crashes.LogError("Test Log Error",new Dictionary<string,string>(){{"user_id","1"}}, this.GetType().FullName);
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private async void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        await Crashes.LogError( LogMessage);
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }
    
    private async void OnGenerateTestCrash(object sender, EventArgs e)
    {
        await Crashes.GenerateTestCrash();
        await DisplayAlert("Info", "LogError Sent", "Ok");
    }

    private void MessageInputView_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _logMessage = e.NewTextValue;
    }
}