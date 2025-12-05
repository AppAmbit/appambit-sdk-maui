using AppAmbitSdkMaui;

namespace AppAmbitTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        //Uncomment the line for automatic session management
        //Analytics.EnableManualSession();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("e3057ace-cd42-4a0c-8ac8-fe3f6d58a2e6");
        
        return builder.Build();
    }
}