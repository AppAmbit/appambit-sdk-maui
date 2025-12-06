using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AppAmbitTestingAppAvalonia.Views;

public partial class AlertWindow : Window
{
    public AlertWindow()
    {
        InitializeComponent();
    }

    public AlertWindow(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
