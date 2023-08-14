using System;
using System.Reflection;
using Kava.Helpers;

namespace Kava.Mvvm
{
	public static class MauiAppBuilder_Extensions
	{
        public static void RegisterRoutes(this MauiAppBuilder mab)
        {
            ReflectionHelper
                .GetTypesFrom<BaseViewModel>(Assembly.GetCallingAssembly())
                ?.ForEach(superClass =>
                {
                    //dynamic v2 = superClass.GetType().GetProperty("Value").PropertyType;
                    //superClass.genericTy
                    
                    //var type = default(superClass);
                    //dynamic singletonVm = superClass;
                    var registerRouteAttribute = Attribute.GetCustomAttribute(superClass, typeof(RegisterRouteAttribute)) as RegisterRouteAttribute;

                    mab.RegisterRoute(registerRouteAttribute!.Route, registerRouteAttribute!.PageType);
                    //mab.RegisterVM<v2>();
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

        //public static void RegisterView<T, TViewModel>(this MauiAppBuilder mab, Type pageType) where T : BaseContentPage<TViewModel>
        //{
        //    mab.Services.AddTransient<T>();
        //}
    }
}

