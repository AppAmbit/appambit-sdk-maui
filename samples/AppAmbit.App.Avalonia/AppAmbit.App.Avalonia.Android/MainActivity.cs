using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using AppAmbitAvalonia;

namespace AppAmbitTestingAppAvalonia.Android;

[Activity(
    Label = "AppAmbitTestingAppAvalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        AppAmbitSdk.Start("<YOUR_APPKEY>");
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
