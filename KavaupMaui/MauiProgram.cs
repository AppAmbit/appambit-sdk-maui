using System.Reflection;
using CommunityToolkit.Maui;
using Kava;
using Kava.Logging.CrashReporter;
using Kava.Oauth;
using Kava.Storage;
using KavaupMaui.ViewModels;
using KavaupMaui.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Kava.Mvvm;

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
		builder
			.RegisterDI()
			.RegisterVM()
			.RegisterViews()
			.RegisterRoutes();

#if DEBUG
        builder.Logging.AddDebug();
#endif
		KavaCrashReporter.Init();
		return builder.Build();
	}
	public static MauiAppBuilder RegisterDI(this MauiAppBuilder mAB)
	{

		var oAuthClientOptions = new OAuthClientOptions
		{
			Domain = "",
			ClientId = "",
			Scope = "openid profile",
			RedirectUri = "KavaupMaui://callback"
		};

        KavaUpMaui.Register(mAB, oAuthClientOptions);
        return mAB;
	}
	public static MauiAppBuilder RegisterVM(this MauiAppBuilder mAB)
	{
		mAB.Services.AddSingleton<MainVM>();
        mAB.Services.AddSingleton<SecondVM>();
        mAB.Services.AddSingleton<ThirdVM>();
        return mAB;
	}
	public static MauiAppBuilder RegisterViews(this MauiAppBuilder mAB)
	{
		mAB.Services.AddTransient<MainPage>();
        mAB.Services.AddTransient<SecondPage>();
        mAB.Services.AddTransient<ThirdPage>();
        return mAB;
	}
	
	

}

