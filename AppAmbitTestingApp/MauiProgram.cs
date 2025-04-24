using AppAmbit;
using Microsoft.Extensions.Logging;

namespace AppAmbitTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        //Comment out this line to have automatic session management
        Analytics.EnableManualSession();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("e310d93f-2497-4b43-86e2-a66d8454a448");

        return builder.Build();
    }
}