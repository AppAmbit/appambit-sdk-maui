using AppAmbitMaui;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;

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
            .UseAppAmbit("<YOUR-APPKEY>");
        
        return builder.Build();
    }
}
