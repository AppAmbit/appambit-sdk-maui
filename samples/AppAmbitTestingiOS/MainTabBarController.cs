using System;
using UIKit;

namespace AppAmbitTestingiOS;

public class MainTabBarController : UITabBarController
{
    static UIImage? SysImg(string name)
    {
        if (OperatingSystem.IsIOSVersionAtLeast(13))
            return UIImage.GetSystemImage(name);
        var fromBundle = UIImage.FromBundle(name);
        return fromBundle;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        var crashes = new CrashesViewController();
        var analytics = new AnalyticsViewController();

        var imgCrashes = SysImg("exclamationmark.triangle");
        var imgAnalytics = SysImg("chart.bar");

        crashes.TabBarItem = imgCrashes != null
            ? new UITabBarItem("Crashes", imgCrashes, 0)
            : new UITabBarItem("Crashes", null, 0);

        analytics.TabBarItem = imgAnalytics != null
            ? new UITabBarItem("Analytics", imgAnalytics, 1)
            : new UITabBarItem("Analytics", null, 1);

        ViewControllers = [crashes, analytics];
    }
}
