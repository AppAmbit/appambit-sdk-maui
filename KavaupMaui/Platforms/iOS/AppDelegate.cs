using Foundation;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace KavaupMaui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		//AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		return base.FinishedLaunching(application, launchOptions);
	}
	
	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
	{
		Console.Out.WriteLine("Hey");
	}
}

