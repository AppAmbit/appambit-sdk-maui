using AppAmbit;
using AppAmbitTestingApp.Utils;

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
            .UseAppAmbit("54c324be-b1a9-49a7-b7e2-6f44d1970669");
        
        return builder.Build();
    }
}