using System.Reflection;
using Kava.Helpers;

namespace Kava.Mvvm;

public static class MauiAppBuilder_Extensions
{
	public static void RegisterRoutes(this MauiAppBuilder mab)
	{
		ReflectionHelper
			.GetTypesFrom<BaseViewModel>(Assembly.GetCallingAssembly())
			?.ForEach(superClass =>
			{
				var registerRouteAttribute = Attribute.GetCustomAttribute(superClass, typeof(RegisterRouteAttribute)) as RegisterRouteAttribute;

				mab.RegisterRoute(registerRouteAttribute!.Route, registerRouteAttribute!.PageType);
			});
	}

	public static void RegisterRoute(this MauiAppBuilder mab, string route, Type pageType)
	{
		Routing.RegisterRoute(route, pageType);
	}

	public static void RegisterVM<TViewModel>(this MauiAppBuilder mab) where TViewModel : class
	{
		mab.Services.AddSingleton<TViewModel>();
	}
}