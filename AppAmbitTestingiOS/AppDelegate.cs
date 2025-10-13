using AppAmbit;
namespace AppAmbitTestingiOS;


[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window
	{
		get;
		set;
	}

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Core.Start("0b2e8bbc-4224-4f57-9b88-a3b0bd112545");
        Window = new UIWindow(UIScreen.MainScreen.Bounds);
        Window.RootViewController = new MainTabBarController();
        Window.MakeKeyAndVisible();        
        return true;
    }
}
