using UIKit;

namespace AppAmbitTestingMacOs;

public partial class SecondViewController : UIViewController
{
    public SecondViewController() : base(nameof(SecondViewController), null)
    {
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View!.BackgroundColor = UIColor.SystemBackground;
        Title = "Change to Second Page";

        var label = new UILabel
        {
            Text = "Second Page",
            TranslatesAutoresizingMaskIntoConstraints = false,
            Font = UIFont.SystemFontOfSize(20),
            TextAlignment = UITextAlignment.Center
        };

        View.AddSubview(label);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            label.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
            label.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
        });
    }

    public override void DidReceiveMemoryWarning()
    {
        base.DidReceiveMemoryWarning();
    }
}
