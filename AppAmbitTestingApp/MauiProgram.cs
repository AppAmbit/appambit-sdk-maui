using AppAmbit;

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
            .UseAppAmbit("196dad5f-73de-410e-86c5-3463d13a23a5");
        
        return builder.Build();
    }
}