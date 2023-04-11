using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KavaupMaui.ViewModels;

public class MainVM  : ObservableObject
{
  int _counter;

  public int Counter
  {
    get => _counter;
    private set => SetProperty(ref _counter, value);
  }
  public ICommand AddCommand { get; private set; }
  public MainVM()
  {
    SetupCommands();
  }
  private void SetupCommands()
  {
    AddCommand = new AsyncRelayCommand(AddCounter);
  }
  private async Task AddCounter()
  {
    Counter++;
  }

}
