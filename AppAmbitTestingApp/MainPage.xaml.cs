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
        Logging.LogAsync(LogTitle, LogMessage, LogType.Error);
    }
    
    private void OnTestCrashLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync(LogTitle, LogMessage, LogType.Crash);
    }
    
    private void OnTestInfoLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync(LogTitle, LogMessage, LogType.Information);
    }
    
    private void OnTestDebugLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync(LogTitle, LogMessage, LogType.Debug);
    }
    
    private void OnTestWarnLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync(LogTitle, LogMessage, LogType.Warning);
    }
    
    private void OnTestSendingFileAndSummaryClicked(object sender, EventArgs e)
    {
        Core.OnStart("40ae5a38-d1b4-4783-977b-1e24570c834d");
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