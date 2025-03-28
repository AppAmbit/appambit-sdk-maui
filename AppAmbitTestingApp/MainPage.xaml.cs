using System.ComponentModel;
using AppAmbit;
using AppAmbit.Models.Logs;

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
    
    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = this;
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private async void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        Crashes.LogError( LogMessage, LogType.Error);
    }
    
    private async void OnTestCrashLogClicked(object sender, EventArgs e)
    {
        Crashes.LogError( LogMessage, LogType.Crash);
    }
    
    private void OnGenerateTestCrash(object sender, EventArgs e)
    {
        Crashes.GenerateTestCrash();
    }

    private void MessageInputView_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _logMessage = e.NewTextValue;
    }
}