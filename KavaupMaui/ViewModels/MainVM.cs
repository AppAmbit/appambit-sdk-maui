using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KavaupMaui.API.Interfaces;
using KavaupMaui.Auth;
using KavaupMaui.Constant;
using KavaupMaui.Providers;
using KavaupMaui.Providers.Interfaces;

namespace KavaupMaui.ViewModels;

public class MainVM  : ObservableObject
{
  int _counter;
  private bool _loginBtn = true;
  private bool _loggedInImg;
  private readonly AuthService _authService;
  private readonly ICacheProvider _cacheProvider;
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
  public ICommand AddCommand { get; private set; }
  public ICommand LoginCommand { get; private set; }
  public ICommand LogOutCommand { get; private set; }

  public MainVM(AuthService authService, 
    ICacheProvider cacheProvider)
  {
    _authService = authService;
    // _networkSession = networkSession;
    _cacheProvider = cacheProvider;
    SetupCommands();
  }
  private void SetupCommands()
  {
    LoginCommand = new AsyncRelayCommand(LoginClicked);
    AddCommand = new AsyncRelayCommand(AddCounter);
    LogOutCommand = new AsyncRelayCommand(LogoutClicked);
  }
  private async Task AddCounter()
  {
    Counter++;
  }
  private async Task LoginClicked()
  {
    
    var loginResult = await _authService.LoginAsync();

    if (!loginResult.IsError)
    {
      LoggedInImg = true;
      LoginBtn = false;

    }
    else
    {
      await Application.Current.MainPage.DisplayAlert("Error!", loginResult.ErrorDescription, "Close");
    }
  }
  private async Task LogoutClicked()
  {
    var logoutResult = await _authService.LogoutAsync();

    if (!logoutResult.IsError) {
      LoggedInImg = false;
      LoginBtn = true;
     
    } else {
      await Application.Current.MainPage.DisplayAlert("Error!", logoutResult.ErrorDescription, "OK");
    }
  }
}
