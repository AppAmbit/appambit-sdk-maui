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
            .UseAppAmbit("dcb52a10-3f16-4894-b254-d8bfbc90bf7e");
        
        return builder.Build();
    }
}