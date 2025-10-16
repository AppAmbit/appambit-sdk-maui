using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace AppAmbitTestingAppAndroid;

[Activity(Label = "SecondActivity")]
public class SecondActivity : Activity
{
    int L(string name) => Resources.GetIdentifier(name, "layout", PackageName);
    int I(string name) => Resources.GetIdentifier(name, "id", PackageName);

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(L("activity_second"));

        var btnReturnToMain = FindViewById<Button>(I("btnReturnMain"))!;
        btnReturnToMain.Click += (s, e) =>
        {
            Finish();
            StartActivity(new Intent(this, typeof(MainActivity)));
        };
    }
}
