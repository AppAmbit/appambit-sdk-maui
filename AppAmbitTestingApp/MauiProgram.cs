using AppAmbit;
using Microsoft.Extensions.Logging;

namespace AppAmbitTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        //Comment out this line to have automatic session management
        //Analytics.EnableManualSession();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("84e932d8-b9b9-4025-b574-0e411bbd86dd");
        
        return builder.Build();
    }
}