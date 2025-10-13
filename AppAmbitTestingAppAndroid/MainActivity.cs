using AppAmbit;

namespace AppAmbitTestingAppAndroid;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Core.Start("b52aaf1d-8470-472f-bd1b-66e7278f2097");

        SetContentView(Resource.Layout.activity_main);
    }
}
