namespace Kava.Dialogs;

public interface IDialogService
{
  Task ShowAlertAsync(string title, string message, string btn);

  Task<bool> ShowConfirmationAsync(string title, string message);

  Task<string> ShowActionsAsync(string title, string cancel, string destruction, params string[] buttons);
}
