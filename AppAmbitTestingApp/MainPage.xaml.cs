using System.ComponentModel;
using AppAmbit;

namespace AppAmbitTestingApp;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private string _logTitle = "";

    public string LogTitle
    {
        get => _logTitle;
        set
        {
            if (_logTitle != value)
            {
                _logTitle = value;
                OnPropertyChanged();
            }
        }
    }
    
    private string _logMessage = "";

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
    
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        Crashes.TrackError(LogTitle, LogMessage, LogType.Error);
    }
    
    private void OnTestCrashLogClicked(object sender, EventArgs e)
    {
        Crashes.TrackError(LogTitle, LogMessage, LogType.Crash);
    }
    
    private void OnTestInfoLogClicked(object sender, EventArgs e)
    {
        Crashes.TrackError(LogTitle, LogMessage, LogType.Information);
    }
    
    private void OnTestDebugLogClicked(object sender, EventArgs e)
    {
        var exception = new NullReferenceException();
        Crashes.TrackError(LogTitle, LogMessage, LogType.Debug);
        Crashes.TrackError(exception);
        Crashes.TrackError(exception,  new Dictionary<string, string>()
        {
            { "Category", "Music" },
        });
    }
    
    private void OnTestWarnLogClicked(object sender, EventArgs e)
    {
        Crashes.TrackError(LogTitle, LogMessage, LogType.Warning);
    }
    
    private void OnGenerateTestCrash(object sender, EventArgs e)
    {
        Crashes.GenerateTestCrash();
    }

    private void TitleInputView_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _logTitle = e.NewTextValue;
    }

    private void MessageInputView_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _logMessage = e.NewTextValue;
    }
}