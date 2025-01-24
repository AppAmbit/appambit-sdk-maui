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
        
        Core.OnStart("05fa9fec-5c5f-4163-b72b-105fb5bea4d5");
    }

    protected override void OnResume()
    {
        base.OnResume();

        Core.OnResume();
    }
    
    protected override void OnSleep()
    {
        base.OnSleep();

        Core.OnSleep();
    }
}