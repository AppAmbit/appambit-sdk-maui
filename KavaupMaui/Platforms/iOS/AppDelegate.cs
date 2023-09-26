using Foundation;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace KavaupMaui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

