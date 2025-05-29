using AppAmbit;
using Microsoft.Extensions.Logging;

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
            .UseAppAmbit("9973a986-eb14-4aa2-965e-3880a5d354d2");
        
        return builder.Build();
    }
}