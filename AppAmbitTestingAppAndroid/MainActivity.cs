using Android.Content;
using AppAmbit;
using Android.Views;

namespace AppAmbitTestingAppAndroid;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    FrameLayout? _container;
    View? _viewCrashes;
    View? _viewAnalytics;

    int L(string name) => Resources.GetIdentifier(name, "layout", PackageName);
    int I(string name) => Resources.GetIdentifier(name, "id", PackageName);

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        AppAmbitSdk.Start("196dad5f-73de-410e-86c5-3463d13a23a5");

        SetContentView(L("activity_main"));

        _container = FindViewById<FrameLayout>(I("content_container"));

        var inflater = LayoutInflater.From(this);
        _viewCrashes   = inflater?.Inflate(L("fragment_crashes"), _container, false);
        _viewAnalytics = inflater?.Inflate(L("fragment_analytics"), _container, false);

        WireCrashesView(_viewCrashes!);
        WireAnalyticsView(_viewAnalytics!);

        var btnCrashes   = FindViewById<Button>(I("btn_nav_crashes"))!;
        var btnAnalytics = FindViewById<Button>(I("btn_nav_analytics"))!;

        btnCrashes.Click   += (s, e) => ShowView(_viewCrashes!);
        btnAnalytics.Click += (s, e) => ShowView(_viewAnalytics!);

        ShowView(_viewCrashes!);
    }

    void ShowView(View v)
    {
        _container!.RemoveAllViews();
        _container.AddView(v);
    }

    void WireAnalyticsView(View root)
    {
        Button B(string id) => root.FindViewById<Button>(I(id))!;
        var btnStartSession                 = B("btnStartSession");
        var btnEndSession                   = B("btnEndSession");
        var btnGenerate30DaysTestSessions   = B("btnGenerate30DaysTestSessions");
        var btnClearToken                   = B("btnClearToken");
        var btnTokenRenew                   = B("btnTokenRenew");
        var btnEventWProperty               = B("btnEventWProperty");
        var btnDefaultClickedEventWProperty = B("btnDefaultClickedEventWProperty");
        var btnMax300LengthEvent            = B("btnMax300LengthEvent");
        var btnMax20PropertiesEvent         = B("btnMax20PropertiesEvent");
        var btn220BatchEvents               = B("btn220BatchEvents");
        var btnSecondActivity               = B("btnSecondActivity");

        btnStartSession.Click += (s, e) => { try { Analytics.StartSession(); } catch { } };
        btnEndSession.Click   += (s, e) => { try { Analytics.EndSession(); } catch { } };

        btnEventWProperty.Click += (s, e) =>
        {
            var map = new Dictionary<string, string> { { "Count", "41" } };
            Analytics.TrackEvent("ButtonClicked", map);
            Toast.MakeText(this, "OnClick event generated", ToastLength.Short)?.Show();
        };

        btnDefaultClickedEventWProperty.Click += (s, e) =>
        {
            Analytics.GenerateTestEvent();
            Toast.MakeText(this, "Event generated", ToastLength.Short)?.Show();
        };

        btnMax300LengthEvent.Click += (s, e) =>
        {
            var props = new Dictionary<string, string>();
            string c300 = new string(Enumerable.Repeat('1', 300).ToArray());
            string c302 = new string(Enumerable.Repeat('2', 302).ToArray());
            props[c300] = c300; props[c302] = c302;
            Analytics.TrackEvent(c300, props);
            Toast.MakeText(this, "1 event generated", ToastLength.Short)?.Show();
        };

        btnMax20PropertiesEvent.Click += (s, e) =>
        {
            var props = Enumerable.Range(1, 25).ToDictionary(i => i.ToString("00"), i => i.ToString("00"));
            Analytics.TrackEvent("TestMaxProperties", props);
            Toast.MakeText(this, "1 event generated", ToastLength.Short)?.Show();
        };

        btnClearToken.Click += (s, e) => Analytics.ClearToken();

        btnTokenRenew.Click += async (s, e) =>
        {
            Analytics.ClearToken();

            var errorTasks = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
            {
                var props = new Dictionary<string, string> { { "user_id", "1" } };
                Crashes.LogError("Sending 5 errors after an invalid token", props);
            })).ToArray();
            await Task.WhenAll(errorTasks);

            Analytics.ClearToken();

            var eventTasks = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
            {
                var ev = new Dictionary<string, string> { { "Test Token", "5 events sent" } };
                Analytics.TrackEvent("Sending 5 events after an invalid token", ev);
            })).ToArray();
            await Task.WhenAll(eventTasks);

            Toast.MakeText(this, "5 events and errors sent", ToastLength.Long)?.Show();
        };

        btnSecondActivity.Click += (s, e) =>
        {
            Finish();
            StartActivity(new Intent(this, typeof(SecondActivity)));
        };
    }

    void WireCrashesView(View root)
    {
        T Find<T>(string id) where T : View => (T)root.FindViewById(I(id))!;
        var btnDidCrash                   = Find<Button>("btnDidCrash");
        var btnSendCustomLogError         = Find<Button>("btnSendCustomLogError");
        var btnSendDefaultLogError        = Find<Button>("btnSendDefaultLogError");
        var btnSendExceptionLogError      = Find<Button>("btnSendExceptionLogError");
        var btnSetUserId                  = Find<Button>("btnSetUserId");
        var btnSetUserEmail               = Find<Button>("btnSetUserEmail");
        var btnThrowNewCrash              = Find<Button>("btnThrowNewCrash");
        var btnGenerateTestCrash          = Find<Button>("btnGenerateTestCrash");

        var etUserId           = Find<EditText>("etUserId");
        var etUserEmail        = Find<EditText>("etUserEmail");
        var etCustomLogErrorText= Find<EditText>("etCustomLogErrorText");

        etUserId.Text = Guid.NewGuid().ToString();
        etUserEmail.Text = "test@gmail.com";
        etCustomLogErrorText.Text = "Test Log Message";

        btnDidCrash.Click += async (s, e) =>
        {
            bool did = await Task.Run(() => Crashes.DidCrashInLastSession());
            ShowAlert("Crash", did ? "Application crashed in the last session"
                                   : "Application did not crash in the last session");
        };

        btnSendCustomLogError.Click += (s, e) =>
        {
            var msg = etCustomLogErrorText.Text ?? "Test Log Message";
            Task.Run(() => Crashes.LogError(msg));
            ShowAlert("Info", "LogError sent");
        };

        btnSendDefaultLogError.Click += (s, e) =>
        {
            Task.Run(() =>
            {
                var props = new Dictionary<string, string> { { "user_id", "1" } };
                Crashes.LogError("Test Log Error", props);
            });
            ShowAlert("Info", "Test Default LogError sent");
        };

        btnSendExceptionLogError.Click += (s, e) =>
        {
            try { throw new NullReferenceException(); }
            catch (Exception ex)
            {
                var props = new Dictionary<string, string> { { "user_id", "1" } };
                Crashes.LogError(ex, props);
                ShowAlert("Info", "Test Exception LogError sent");
            }
        };

        btnSetUserId.Click                  += (s, e) => { Analytics.SetUserId(etUserId.Text); ShowAlert("Info", "User ID changed"); };
        btnSetUserEmail.Click               += (s, e) => { Analytics.SetUserEmail(etUserEmail.Text); ShowAlert("Info", "User email changed"); };
        btnThrowNewCrash.Click              += (s, e) => { throw new NullReferenceException(); };
        btnGenerateTestCrash.Click          += (s, e) => Crashes.GenerateTestCrash();
    }

    void ShowAlert(string title, string message)
    {
        RunOnUiThread(() =>
        {
            new AlertDialog.Builder(this)
                .SetTitle(title)!
                .SetMessage(message)!
                .SetPositiveButton("OK", (s, e) => { })!
                .Show();
        });
    }
}
