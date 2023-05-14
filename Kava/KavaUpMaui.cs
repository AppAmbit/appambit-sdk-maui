using System;
using Kava.API;
using Kava.Dialogs;
using Kava.Oauth;
using Kava.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Kava
{
	public static class KavaUpMaui
	{
        public static Application Application;

		public static void Register(MauiAppBuilder mAB, OAuthClientOptions oAuthClientOptions = null)
		{
            Akavache.Registrations.Start(AkavacheCacheProvider.AppName);

            var akavacheCacheProvider = new AkavacheCacheProvider();
            mAB.Services.AddSingleton<ICacheProvider>(akavacheCacheProvider);
            mAB.Services.AddSingleton<ISessionManager, SessionManager>();
            mAB.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            mAB.Services.AddSingleton<IWebAPIService, WebAPIService>();
            mAB.Services.AddSingleton<IDialogService, DialogService>();

            if (oAuthClientOptions != null)
            {
                mAB.Services.AddSingleton<OAuthClientOptions>(oAuthClientOptions);
               
                mAB.Services.AddSingleton<IOAuthService>(new OAuthService(oAuthClientOptions, akavacheCacheProvider));
            }
        }
	}
}

