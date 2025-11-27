using Foundation;
using UIKit;

namespace AppAmbitTestingiOS;

public class SecondView : UIViewController
{
    UILabel label;

    public SecondView()
    {
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View.BackgroundColor = UIColor.SystemBackground;
        Title = "SecondView";

        label = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Second View",
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.SystemFontOfSize(24, UIFontWeight.Semibold)
        };

        var close = new UIButton(UIButtonType.System);
        close.TranslatesAutoresizingMaskIntoConstraints = false;
        close.SetTitle("Close", UIControlState.Normal);
        close.TouchUpInside += (s, e) =>
        {
            DismissViewController(true, null);
        };

        View.AddSubview(label);
        View.AddSubview(close);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            label.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
            label.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),

            close.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 16),
            close.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 16)
        });
    }
}
