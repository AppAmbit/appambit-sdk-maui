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
        
        Core.OnStart("760eb415-e053-4a2c-b1ad-d18958064736");
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