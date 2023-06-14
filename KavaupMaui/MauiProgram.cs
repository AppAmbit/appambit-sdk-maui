using System.Reflection;
using CommunityToolkit.Maui;
using IdentityModel.Jwk;
using IdentityModel.OidcClient;
using Kava;
using Kava.Oauth;
using Kava.Storage;
using KavaupMaui.ViewModels;
using KavaupMaui.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KavaupMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
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
			.UseMauiCommunityToolkit()
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
		//Google OAuth test
        var oAuthClientOptions = new OAuthClientOptions
		{
			Authority = "accounts.google.com",
			ClientId = "791579469784-857mhv6435tbdq3ut3jbc8b25mddnb2g.apps.googleusercontent.com",
			Scope = "openid profile",
			RedirectUri = "com.googleusercontent.apps.791579469784-857mhv6435tbdq3ut3jbc8b25mddnb2g:/oauth2redirect",
			AuthorizeEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
			TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token"
        };

		
        KavaUpMaui.Register(mAB, oAuthClientOptions);
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

