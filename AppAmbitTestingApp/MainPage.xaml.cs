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

    private async void OnSendTestLog(object sender, EventArgs e)
    {
        await Crashes.LogError("Test Log Error",new Dictionary<string, object>(){{"user_id",1}});
    }

    private async void OnSendTestException(object sender, EventArgs e)
    {
        try
        {
            throw new NullReferenceException();
        }
        catch (Exception exception)
        {
            await Crashes.LogError(exception,new Dictionary<string, object>(){{"user_id",1}});
        }
    }

    private async void OnSendTestLogWithClassFQN(object sender, EventArgs e)
    {
        await Crashes.LogError("Test Log Error",new Dictionary<string, object>(){{"user_id",1}}, this.GetType().FullName);
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private async void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        Crashes.LogError( LogMessage);
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