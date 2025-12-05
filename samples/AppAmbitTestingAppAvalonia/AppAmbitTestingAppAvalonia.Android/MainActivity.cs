using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using AppAmbitSdkAvalonia;

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
        AppAmbitSdk.Start("115521de-1dd4-45a9-9f7e-938189383b99");
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
