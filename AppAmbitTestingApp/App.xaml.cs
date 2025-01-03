using AppAmbit;

namespace AppAmbitTestingApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        
        Core.OnStart("e1a848f4-46b1-45e4-8dcb-128510611c02");
    }

    
    protected override void OnSleep()
    {
        base.OnSleep();

        Core.OnSleep();
    }
}