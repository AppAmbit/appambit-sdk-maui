using iOSAppAmbit;
using Shared.Models.Logs;

namespace iOSAppAmbitTestingApp;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public int Height { get; set; } = 100;
    
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Window = new UIWindow(UIScreen.MainScreen.Bounds);

        var vc = new UIViewController();
        
        
        var scrollView = new UIScrollView(new CGRect(0, 0, Window.Frame.Width, Window.Frame.Height))
        {
            BackgroundColor = UIColor.White,
            AutoresizingMask = UIViewAutoresizing.FlexibleDimensions
        };
        scrollView.ContentSize = new CGSize(Window.Frame.Width, Window.Frame.Height);
        var view = new UIView(new CGRect(0, 0, Window.Frame.Width, Window.Frame.Height))
        {
            BackgroundColor = UIColor.White,
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
        };

        var button1 = CreateUIButton("Test adding error log", OnTestErrorLogClicked);
        var button2 = CreateUIButton("Test adding crash log", OnTestCrashLogClicked);
        var button3 = CreateUIButton("Test adding info log", OnTestInfoLogClicked);
        var button4 = CreateUIButton("Test adding debug log", OnTestDebugLogClicked);
        var button5 = CreateUIButton("Test adding warn log", OnTestWarnLogClicked);
        var button6 = CreateUIButton("Test sending file and summary", OnTestSendingFileAndSummaryClicked);
        var button7 = CreateUIButton("Test logging crash", OnCounterClicked);
        
        view.AddSubview(button1);
        view.AddSubview(button2);
        view.AddSubview(button3);
        view.AddSubview(button4);
        view.AddSubview(button5);
        view.AddSubview(button6);
        view.AddSubview(button7);

        scrollView.AddSubview(view);
        vc.View!.AddSubview(scrollView);
        
        Window.RootViewController = vc;
        Window.MakeKeyAndVisible();

        Core.OnStart("de84a1ca-64de-4d85-8253-8d2626fb662b");

        return true;
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        throw new NullReferenceException();
    }
    
    private void OnTestErrorLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("LogTitle", "LogMessage", LogType.Error);
    }
    
    private void OnTestCrashLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("LogTitle", "LogMessage", LogType.Crash);
    }
    
    private void OnTestInfoLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("LogTitle", "LogMessage", LogType.Information);
    }
    
    private void OnTestDebugLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("LogTitle", "LogMessage", LogType.Debug);
    }
    
    private void OnTestWarnLogClicked(object sender, EventArgs e)
    {
        Logging.LogAsync("LogTitle", "LogMessage", LogType.Warning);
    }
    
    private void OnTestSendingFileAndSummaryClicked(object sender, EventArgs e)
    {
        Core.OnStart("e79ffcef-c6c7-465c-ba5b-a9494c3af84e");
    }
    
    private UIButton CreateUIButton(string title, EventHandler action)
    {
        var button = new UIButton(new CGRect(50, Height,  200, 50))
        {
            BackgroundColor = UIColor.Blue, 
            VerticalAlignment = UIControlContentVerticalAlignment.Center
        };
        button.SetTitle(title, UIControlState.Normal);
        button.TouchUpInside += action;
        Height += 75;
        return button;
    }

    public void Open()
    {
        
    }
    
    // public override void OnActivated(UIApplication application)
    // {
    //     Core.OnResume();
    // }
    
    public override void OnResignActivation(UIApplication application)
    {
        Core.OnSleep();
    }
}