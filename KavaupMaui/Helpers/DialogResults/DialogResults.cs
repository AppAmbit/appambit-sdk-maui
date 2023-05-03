using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace KavaupMaui.Helpers.DialogResults;

public class DialogResults : IDialogResults
{
  public Task ShowAlertAsync(string title, string message, string btn)
  {
    return Application.Current.MainPage.DisplayAlert(title, message, btn);
  }
  
  public Task<bool> ShowConfirmationAsync(string title, string message)
  {
    return Application.Current.MainPage.DisplayAlert(title, message, "ok", "no");
  }
  public Task<string> ShowActionsAsync(string title, string cancel, string destruction, params string[] buttons)
  {
    return Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
  }
}
