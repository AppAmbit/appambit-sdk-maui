using AppAmbit;
using System.Text;
using System.Windows;

namespace AppAmbit.App.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new CrashesPage());
    }

    private void ButtonPage1_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new CrashesPage());
    }

    private void ButtonPage2_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new AnalyticsPage());
    }
}