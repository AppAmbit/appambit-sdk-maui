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
        
        Core.OnStart("40ae5a38-d1b4-4783-977b-1e24570c834d");
    }

    
    protected override void OnSleep()
    {
        base.OnSleep();

        Core.OnSleep();
    }
}