using System.Reflection;
using KavaupMaui.API;
using KavaupMaui.API.Interfaces;
using KavaupMaui.Auth;
using KavaupMaui.Auth.Interfaces;
using KavaupMaui.Helpers.AppName;
using KavaupMaui.Providers;
using KavaupMaui.Providers.Interfaces;
using KavaupMaui.ViewModels;
using KavaupMaui.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace KavaupMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Akavache.Registrations.Start(ApplicationName.AppName);

		#region JSON Settings
		var appSettingsFile = "KavaupMaui.appsettings.json";
#if DEBUG
		appSettingsFile = "KavaupMaui.appsettings.Development.json";
#endif
		using var settings = Assembly.GetExecutingAssembly().GetManifestResourceStream(appSettingsFile);
		var config = new ConfigurationBuilder().AddJsonStream(settings).Build();
		#endregion
		
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).Configuration.AddConfiguration(config);
		builder.RegisterDI().RegisterVM().RegisterViews();
#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}
	public static MauiAppBuilder RegisterDI(this MauiAppBuilder mAB)
	{
		#region AuthSettings
		mAB.Services.AddSingleton(new AuthService(new AuthClientOptions {
		Domain = "",
		ClientId = "",
		Scope = "openid profile",
		RedirectUri = "KavaupMaui://callback"
		}));
		mAB.Services.AddSingleton<IAuthService, AuthService>();
		#endregion
		mAB.Services.AddSingleton<ICacheProvider, AkavacheCacheProvider>();

		return mAB;
	}
	public static MauiAppBuilder RegisterVM(this MauiAppBuilder mAB)
	{
		mAB.Services.AddSingleton<MainVM>();
		return mAB;
	}
	public static MauiAppBuilder RegisterViews(this MauiAppBuilder mAB)
	{
		mAB.Services.AddTransient<MainPage>();
		return mAB;
	}
}

