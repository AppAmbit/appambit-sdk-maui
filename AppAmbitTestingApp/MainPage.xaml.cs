using AppAmbit;
using System.Diagnostics;
using AppAmbit.Services;
using Newtonsoft.Json;
using static AppAmbitTestingApp.FormattedRequestSize;
using static System.Linq.Enumerable;
using AppAmbitTestingApp.Utils;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_inited) return;
        _inited = true;

        try
        {
            await StorableApp.Shared.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StorableApp] init error: {ex}");
        }
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
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await DisplayAlert("Info", "Turn off internet and try again", "Ok");
            return;
        }

        LoggingHandler.ResetTotalSize();

        foreach (int index in Range(start: 1, count: 30))
        {
            var errorsDate = DateTime.UtcNow.AddDays(-(30 - index));

            await StorableApp.Shared.PutSessionData(errorsDate, "start");

            await Crashes.LogError("Test 30 Last Days Errors", createdAt: errorsDate);

            await StorableApp.Shared.UpdateLogsWithCurrentSessionId();

            await StorableApp.Shared.PutSessionData(errorsDate.AddSeconds(2), "end");

            await Task.Delay(500);
        }

        await DisplayAlert("Info", "Logs generated, turn on internet", "Ok");
        ButtonLast30DailyErrors.Padding = 10;
        ButtonLast30DailyErrors.FontSize = 12;
        ButtonLast30DailyErrors.Text = $"Generate the last 30 daily errors ({FormatSize(LoggingHandler.TotalRequestSize)})";
    }


    private async void OnGenerate30daysTestCrash(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await DisplayAlert("Info", "Turn off internet and try again", "Ok");
            return;
        }
        var ex = new NullReferenceException();
        foreach (int index in Range(start: 1, count: 30))
        {
            var crashDate = DateTime.UtcNow.AddDays(-(30 - index));

             await StorableApp.Shared.PutSessionData(crashDate, "start");

             var sessionId = await StorableApp.Shared.GetCurrentOpenSessionIdUnsafeAsync();

            var info = ExceptionModelApp.FromException(ex, deviceId: "iPhone 16 PRO MAX", sessionId);

            info.CreatedAt = crashDate;
            info.CrashLogFile = crashDate.ToString("yyyy-MM-ddTHH:mm:ss") + "_" + index;

            var json = JsonConvert.SerializeObject(info, Formatting.Indented);

            string timestamp = crashDate.ToString("yyyyMMdd_HHmmss");
            string fileName = $"crash_{timestamp}_{index}.json";

            string crashFile = Path.Combine(FileSystem.AppDataDirectory, fileName);

            Debug.WriteLine($"Crash file saved to: {crashFile}");
            await StorableApp.Shared.PutSessionData(crashDate.AddSeconds(2), "end");
            await Task.Delay(100);
            File.WriteAllText(crashFile, json);
        }
        await DisplayAlert("Info", "Crashes generated, turn on internet", "Ok");
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