using AppAmbit.Models.Logs;
using AppAmbit;

namespace AppAmbitTestingApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private void OnTestCustomLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("Test", "Custom Log", LogType.Information);
    }
    
    private void OnTestSendingFileAndSummaryClicked(object sender, EventArgs e)
    {
        Core.OnStart("e1a848f4-46b1-45e4-8dcb-128510611c02");
    }
}