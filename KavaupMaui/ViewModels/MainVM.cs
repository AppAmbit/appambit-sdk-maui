using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kava.API;
using Kava.Dialogs;
using Kava.Logging;
using Kava.Mvvm;
using Kava.Oauth;
using Kava.Storage;
using KavaupMaui.Constant;
using KavaupMaui.Endpoints;
using KavaupMaui.Models;
using KavaupMaui.Views;
using Microsoft.Extensions.Logging;

namespace KavaupMaui.ViewModels;

[RegisterRoute(route: "Main", pageType: typeof(MainPage))]
public class MainVM : BaseViewModel
{
    int _counter;
    private bool _loginBtn = true;
    private bool _loggedInImg;
    private string _testEndpointResult;
    private readonly IOAuthService _authService;
    private readonly ICacheProvider _cacheProvider;
    private readonly IDialogService _dialogResults;
    private readonly IWebAPIService _webAPIService;
    private readonly LogManager _logManager;

    public int Counter
    {
        get => _counter;
        private set => SetProperty(ref _counter, value);
    }

    public bool LoginBtn
    {
        get => _loginBtn;
        private set => SetProperty(ref _loginBtn, value);
    }

    public bool LoggedInImg
    {
        get => _loggedInImg;
        private set => SetProperty(ref _loggedInImg, value);
    }

    public string TestEndpointResult
    {
        get => _testEndpointResult;
        private set => SetProperty(ref _testEndpointResult, value);
    }
    

    public ICommand AddCommand { get; private set; }
    public ICommand LoginCommand { get; private set; }
    public ICommand LogOutCommand { get; private set; }
    public ICommand TestEndpointCommand { get; private set; }
    public ICommand TestLogCommand { get; private set; }
    public ICommand TestAutoDeleteCommand { get; private set; }
    public ICommand TestClearLogCommand { get; private set; }
    public ICommand GoToNextPage { get; private set; }

    public MainVM(IOAuthService authService,
                ICacheProvider cacheProvider,
                IDialogService dialogResults,
                IWebAPIService webAPIService,
                LogManager logManager)
    {
        _authService = authService;
        _cacheProvider = cacheProvider;
        _dialogResults = dialogResults;
        _webAPIService = webAPIService;
        _logManager = logManager;
        SetupCommands();
    }

    private void SetupCommands()
    {
        LoginCommand = new AsyncRelayCommand(LoginClicked);
        AddCommand = new AsyncRelayCommand(AddCounter);
        LogOutCommand = new AsyncRelayCommand(LogoutClicked);
        TestEndpointCommand = new AsyncRelayCommand(TestEndpointClicked);
        TestLogCommand = new AsyncRelayCommand(TestLogClicked);
        TestAutoDeleteCommand = new AsyncRelayCommand(TestAutoDeletingLog);
        TestClearLogCommand = new AsyncRelayCommand(TestClearLogs);
        GoToNextPage = new AsyncRelayCommand(GoToSecondPage);
    }

    private async Task AddCounter()
    {
        Counter++;
    }

    private async Task TestEndpointClicked()
    {
        var response = await _webAPIService.MakeRequest<TestResponse>(new TestEndpoint(), new CancellationToken());
        TestEndpointResult = response?.Data;
    }

    private async Task TestLogClicked()
    {
        _logManager.Log("Test log", level: LogLevel.Information);
    }
    
    private async Task DisplayMessage(string message) => await _dialogResults.ShowAlertAsync("Message", message, "Close");
    
    
    private async Task TestAutoDeletingLog()
    {
#pragma warning disable
        _logManager.LogSizeKB = 1; //1 kilobytes
        Task.Run(async () =>
        {
            var j = 10000000;
            for (var i = 0; i < j; i++)
            {
                _logManager.Log("Test log", level: LogLevel.Information);
            }
        });
    }

    private async Task TestClearLogs()
    {
        await Task.Delay(100);
        _logManager.ClearLogs();
    }

    private async Task LoginClicked()
    {
        //TODO:setup an OAuth account to test against
        //this won't work without proper configuration
        var loginResult = await _authService.LoginAsync();
        if (!loginResult.IsError)
        {
            LoggedInImg = true;
            LoginBtn = false;
        }
        else
        {
            await _dialogResults.ShowAlertAsync("Error!", loginResult.ErrorDescription, "Close");
        }
    }
    private async Task LogoutClicked()
    {
        var logoutResult = await _authService.LogoutAsync();

        if (!logoutResult.IsError)
        {
            LoggedInImg = false;
            LoginBtn = true;
        }
        else
        {
            await _dialogResults.ShowAlertAsync("Error!", logoutResult.ErrorDescription, "Close");
        }
    }

    public async Task GoToSecondPage()
    {
        //await Shell.Current.GoToAsync("Second");
        await Shell.Current.Navigate(typeof(SecondVM));
    }
}
