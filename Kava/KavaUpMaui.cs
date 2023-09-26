using Kava.Dialogs;
using Kava.Logging;
using Kava.OAuth;
using Kava.Storage;

namespace Kava;

public static class KavaUpMaui
{
	public static Application Application;

	public static void Register(MauiAppBuilder mAB, OAuthClientOptions oAuthClientOptions = null)
	{
		Akavache.Registrations.Start(AkavacheCacheProvider.AppName);

		var akavacheCacheProvider = new AkavacheCacheProvider();
		mAB.Services.AddSingleton<ICacheProvider>(akavacheCacheProvider);
		mAB.Services.AddSingleton<IDialogService, DialogService>();
		mAB.Services.AddSingleton<ILogService, KavaLogger>();
		mAB.Services.AddSingleton<INetworkLogService, MockNetworkLogService>();
		mAB.Services.AddSingleton<LogManager, LogManager>();

		if (oAuthClientOptions != null)
		{
			mAB.Services.AddSingleton<OAuthClientOptions>(oAuthClientOptions);

			mAB.Services.AddSingleton<IOAuthService>(new OAuthService(oAuthClientOptions, akavacheCacheProvider));
		}
	}
}