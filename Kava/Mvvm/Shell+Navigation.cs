using System;
namespace Kava.Mvvm
{
	public static class Shell_Navigation
	{
		public static Task Navigate(this Shell shell, Type viewModelType, params object[] arguments)
		{
            var registerRouteAttribute = Attribute.GetCustomAttribute(viewModelType, typeof(RegisterRouteAttribute)) as RegisterRouteAttribute;
			if (arguments.Count<object>() > 0)
				BaseViewModel.PrepareArguments(viewModelType, arguments);
			return shell.GoToAsync(registerRouteAttribute?.Route);
		}
	}
}

