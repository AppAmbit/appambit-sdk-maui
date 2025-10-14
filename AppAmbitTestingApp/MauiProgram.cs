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
            .UseAppAmbit("e86d9825-0222-404e-84be-48c42f10fec9");
        
        return builder.Build();
    }
}