using AppAmbit;
using System.Diagnostics;
using AppAmbit.Services;
using Newtonsoft.Json;
using static AppAmbitTestingApp.FormattedRequestSize;
using static System.Linq.Enumerable;

using AppAmbitTestingApp.Models;

namespace AppAmbitTestingApp;

public partial class MainPage : ContentPage
{
    private bool _inited;

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
        LoggingHandler.ResetTotalSize();

        await DisplayAlert("Info", "Turn off internet", "Ok");
        foreach (int index in Range(1, 220))
        {
            await Crashes.LogError("Test Batch LogError");
        }
        await DisplayAlert("Info", "Logs generated", "Ok");
        await DisplayAlert("Info", "Turn on internet to send the logs", "Ok");

        ButtonBatchUpload.Padding = 10;
        ButtonBatchUpload.FontSize = 12;
        ButtonBatchUpload.Text = $"Generate Logs for Batch upload ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void OnHasCrashedTheLastSession(object? sender, EventArgs eventArgs)
    {
        if (await Crashes.DidCrashInLastSession())
        {
            await DisplayAlert("Info", "Application crashed in the last session", "Ok");
        }
        else
        {
            await DisplayAlert("Info", "Application did not crash in the last session", "Ok");
        }
    }

    private async void OnChangeUserId(object? sender, EventArgs e)
    {
        Analytics.SetUserId(UserId);
        await DisplayAlert("Info", "User id changed", "Ok");
    }

    private async void OnChangeUserEmail(object? sender, EventArgs e)
    {
        Analytics.SetUserEmail(UserEmail);
        await DisplayAlert("Info", "User email changed", "Ok");
    }

    private async void OnSendTestLog(object sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();
        await Crashes.LogError("Test Log Error", new Dictionary<string, string>() { { "user_id", "1" } });
        await DisplayAlert("Info", "LogError Sent", "Ok");
        ButtonDefaultLogError.Padding = 10;
        ButtonDefaultLogError.FontSize = 12;
        ButtonDefaultLogError.Text = $"Send Default LogError ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void OnSendTestException(object sender, EventArgs e)
    {
        try
        {
            throw new NullReferenceException();
        }
        catch (Exception exception)
        {
            LoggingHandler.ResetTotalSize();
            await Crashes.LogError(exception, new Dictionary<string, string>() { { "user_id", "1" } });
            await DisplayAlert("Info", "LogError Sent", "Ok");
            ButtonSendTestException.Padding = 10;
            ButtonSendTestException.FontSize = 12;
            ButtonSendTestException.Text = $"Send Exception LogError ({FormatSize(LoggingHandler.TotalRequestSize)})";
        }
    }

    private async void OnSendTestLogWithClassFQN(object sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();
        await Crashes.LogError("Test Log Error", new Dictionary<string, string>() { { "user_id", "1" } }, this.GetType().FullName);
        await DisplayAlert("Info", "LogError Sent", "Ok");
        ButtonTestLogWithClassFQN.Padding = 10;
        ButtonTestLogWithClassFQN.FontSize = 12;
        ButtonTestLogWithClassFQN.Text = $"Send ClassInfo LogError ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }

    private async void OnGenerate30daysTestErrors(object sender, EventArgs e)
    {
        
    }


    private async void OnGenerate30daysTestCrash(object sender, EventArgs e)
    {
        
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }

    private async void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        LoggingHandler.ResetTotalSize();
        await Crashes.LogError(LogMessage);
        await DisplayAlert("Info", "LogError Sent", "Ok");
        ButtonCustomLogError.Padding = 10;
        ButtonCustomLogError.FontSize = 12;
        ButtonCustomLogError.Text = $"Send Custom LogError ({FormatSize(LoggingHandler.TotalRequestSize)})";
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