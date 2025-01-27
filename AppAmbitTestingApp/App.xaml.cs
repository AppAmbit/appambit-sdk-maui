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
        
        Core.OnStart("ea06903e-6d89-467b-b493-3f265b2dde05");
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