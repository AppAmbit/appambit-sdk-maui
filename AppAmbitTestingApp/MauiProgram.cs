using AppAmbit;
using Microsoft.Extensions.Logging;

namespace AppAmbitTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("cbe81c2b-3fa6-45ac-9ed9-7c9792e4843e");
        
        return builder.Build();
    }
}