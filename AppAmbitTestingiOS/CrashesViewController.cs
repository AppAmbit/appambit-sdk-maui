using System.Diagnostics;
using AppAmbit;

namespace AppAmbitTestingiOS;

public class CrashesViewController : UIViewController
{
    string userId = Guid.NewGuid().ToString();
    string email = "test@gmail.com";
    string messgeCutsom = "Test Log Message";

    UITextField userIdField;
    UITextField emailField;
    UITextField messageField;

    UIColor BackgroundCompat()
    {
        if (OperatingSystem.IsIOSVersionAtLeast(13)) return UIColor.SystemBackground;
        return UIColor.White;
    }

    UIColor BlueCompat()
    {
        if (OperatingSystem.IsIOSVersionAtLeast(13)) return UIColor.SystemBlue;
        return UIColor.FromRGB(0, 122, 255);
    }

    UIColor Gray6Compat()
    {
        if (OperatingSystem.IsIOSVersionAtLeast(13)) return UIColor.SystemGray6;
        return UIColor.FromRGB(242, 242, 247);
    }

UIButton MakeButton(string title)
{
    var b = new UIButton(UIButtonType.System);
    b.SetTitle(title, UIControlState.Normal);
    b.BackgroundColor = BlueCompat();
    b.SetTitleColor(UIColor.White, UIControlState.Normal);
    b.Layer.CornerRadius = 8;

    if (OperatingSystem.IsIOSVersionAtLeast(15))
    {
        b.TranslatesAutoresizingMaskIntoConstraints = false;
        b.WidthAnchor.ConstraintGreaterThanOrEqualTo(160).Active = true;
    }
    else
    {
        b.ContentEdgeInsets = new UIEdgeInsets(12, 24, 12, 24);
    }

    return b;
}




    UIButton MakeDisabledGray(string title)
    {
        var b = new UIButton(UIButtonType.System);
        b.SetTitle(title, UIControlState.Normal);
        b.BackgroundColor = UIColor.FromRGB(96, 120, 141);
        b.SetTitleColor(Gray6Compat(), UIControlState.Normal);
        b.Layer.CornerRadius = 8;
        if (!OperatingSystem.IsIOSVersionAtLeast(15))
            b.ContentEdgeInsets = new UIEdgeInsets(12, 12, 12, 12);
        b.Enabled = false;
        return b;
    }

    UIView MakeTextField(string placeholder, out UITextField field, UIKeyboardType keyboardType)
    {
        field = new UITextField
        {
            Placeholder = placeholder,
            Text = placeholder == "User Id" ? userId : placeholder == "User email" ? email : messgeCutsom,
            AutocapitalizationType = UITextAutocapitalizationType.None,
            AutocorrectionType = UITextAutocorrectionType.No,
            KeyboardType = keyboardType,
            BorderStyle = UITextBorderStyle.RoundedRect,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        var v = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
        v.Add(field);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            field.TopAnchor.ConstraintEqualTo(v.TopAnchor),
            field.LeadingAnchor.ConstraintEqualTo(v.LeadingAnchor),
            field.TrailingAnchor.ConstraintEqualTo(v.TrailingAnchor),
            field.BottomAnchor.ConstraintEqualTo(v.BottomAnchor),
            field.HeightAnchor.ConstraintEqualTo(40),
        });

        return v;
    }

    void ShowAlert(string title, string message)
    {
        var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
        PresentViewController(alert, true, null);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View.BackgroundColor = BackgroundCompat();

        var scroll = new UIScrollView { TranslatesAutoresizingMaskIntoConstraints = false };
        var container = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
        var stack = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 25,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        var btnDidCrash = MakeButton("Did the app crash during your last session?");
        btnDidCrash.TouchUpInside += async (s, e) =>
        {
            var did = await Crashes.DidCrashInLastSession();
            var msg = did ? "Application crashed in the last session" : "Application did not crash in the last session";
            ShowAlert("Info", msg);
        };

        var userIdView = MakeTextField("User Id", out userIdField, UIKeyboardType.Default);
        var btnChangeUserId = MakeButton("Change user id");
        btnChangeUserId.TouchUpInside += (s, e) =>
        {
            userId = userIdField.Text ?? "";
            Analytics.SetUserId(userId);
            Debug.WriteLine("User ID set successfully");
        };

        var emailView = MakeTextField("User email", out emailField, UIKeyboardType.EmailAddress);
        var btnChangeEmail = MakeButton("Change user email");
        btnChangeEmail.TouchUpInside += (s, e) =>
        {
            email = emailField.Text ?? "";
            Analytics.SetUserEmail(email);
        };

        var msgView = MakeTextField("Test Log Message", out messageField, UIKeyboardType.Default);
        var btnSendCustom = MakeButton("Send Custom LogError");
        btnSendCustom.TouchUpInside += async (s, e) =>
        {
            messgeCutsom = messageField.Text ?? "";
            await Crashes.LogError(messgeCutsom);
            ShowAlert("Info", "LogError Sent");
        };

        var btnSendDefault = MakeButton("Send Default LogError");
        btnSendDefault.TouchUpInside += async (s, e) =>
        {
            var props = new Dictionary<string, string> { { "user_id", "1" } };
            await Crashes.LogError("Test Log Error", props);
            ShowAlert("Info", "LogError Sent");
        };

        var btnSendException = MakeButton("Send Exception LogError");
        btnSendException.TouchUpInside += async (s, e) =>
        {
            try
            {
                throw new Exception("Test error Exception");
            }
            catch (Exception ex)
            {
                var props = new Dictionary<string, string> { { "user_id", "1" } };
                await Crashes.LogError(ex, props);
                ShowAlert("Info", "LogError Sent");
            }
        };

        var btnClassInfo = MakeButton("Send ClassInfo LogError");
        btnClassInfo.TouchUpInside += async (s, e) =>
        {
            var classFullName = GetType().FullName ?? "CrashesViewController";
            var props = new Dictionary<string, string> { { "user_id", "1" } };
            await Crashes.LogError("Test Log Error", props, classFullName);
            ShowAlert("Info", "LogError Sent");
        };

        var btnThrowCrash = MakeButton("Throw new Crash");
        btnThrowCrash.TouchUpInside += (s, e) =>
        {
            var arr = new int[0];
            var _ = arr[10];
        };

        var btnGenerateTestCrash = MakeButton("Generate Test Crash");
        btnGenerateTestCrash.TouchUpInside += (s, e) =>
        {
            _ = Crashes.GenerateTestCrash();
        };

        var items = new UIView[]
        {
            btnDidCrash,
            userIdView,
            btnChangeUserId,
            emailView,
            btnChangeEmail,
            msgView,
            btnSendCustom,
            btnSendDefault,
            btnSendException,
            btnClassInfo,
            btnThrowCrash,
            btnGenerateTestCrash
        };

        foreach (var v in items) stack.AddArrangedSubview(v);

        View.AddSubview(scroll);
        scroll.AddSubview(container);
        container.AddSubview(stack);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

            container.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor),
            container.LeadingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.LeadingAnchor),
            container.TrailingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TrailingAnchor),
            container.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor),
            container.WidthAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.WidthAnchor),

            stack.TopAnchor.ConstraintEqualTo(container.TopAnchor, 16),
            stack.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor, 16),
            stack.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor, -16),
            stack.BottomAnchor.ConstraintEqualTo(container.BottomAnchor, -16),
        });
    }
}
