// SceneDelegate.cs
using System;
using Foundation;
using UIKit;
using AppAmbit;

namespace AppAmbitTestingMacOs;

[Register ("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate {

	[Export ("window")]
	public UIWindow? Window { get; set; }

	[Export ("scene:willConnectToSession:options:")]
	public void WillConnect (UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
	{
		// Use this method to optionally configure and attach the UIWindow 'Window' to the provided UIWindowScene 'scene'.
		// Since we are not using a storyboard, the 'Window' property needs to be initialized and attached to the scene.
		// This delegate does not imply the connecting scene or session are new (see UIApplicationDelegate 'GetConfiguration' instead).
		if (scene is UIWindowScene windowScene) {
			Window ??= new UIWindow (windowScene);

			// Create a 'UIViewController' with a single 'UILabel'
			var vc = new UIViewController ();
			vc.View!.AddSubview (new UILabel (Window!.Frame) {
				BackgroundColor = UIColor.SystemBackground,
				TextAlignment = UITextAlignment.Center,
				Text = "Hello, Mac Catalyst!",
				AutoresizingMask = UIViewAutoresizing.All,
			});

			UIButton MakeButton(string title)
			{
				var b = new UIButton(UIButtonType.System);
				b.SetTitle(title, UIControlState.Normal);
				b.TranslatesAutoresizingMaskIntoConstraints = false;
				b.BackgroundColor = UIColor.SystemBlue;
				b.SetTitleColor(UIColor.White, UIControlState.Normal);
				b.Layer.CornerRadius = 8;
				b.HeightAnchor.ConstraintEqualTo(44).Active = true;
				return b;
			}

			void Show(string title, string message)
			{
				var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
				vc.PresentViewController(alert, true, null);
			}

			var btnDidCrash = MakeButton("Did the app crash during your last session?");
			btnDidCrash.TouchUpInside += async (s, e) =>
			{
				var did = await Crashes.DidCrashInLastSession();
				Show("Info", did ? "Application crashed in the last session" : "Application did not crash in the last session");
			};

			var btnThrowCrash = MakeButton("Throw new Crash");
			btnThrowCrash.TouchUpInside += (s, e) =>
			{
				var arr = new int[0];
				_ = arr[10];
			};

			var btnGenerateTestCrash = MakeButton("Generate Test Crash");
			btnGenerateTestCrash.TouchUpInside += (s, e) =>
			{
				_ = Crashes.GenerateTestCrash();
			};

			var stack = new UIStackView(new UIView[] { btnDidCrash, btnThrowCrash, btnGenerateTestCrash })
			{
				Axis = UILayoutConstraintAxis.Vertical,
				Alignment = UIStackViewAlignment.Fill,
				Distribution = UIStackViewDistribution.Fill,
				Spacing = 16,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			vc.View!.AddSubview(stack);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				stack.TopAnchor.ConstraintEqualTo(vc.View.SafeAreaLayoutGuide.TopAnchor, 24),
				stack.LeadingAnchor.ConstraintEqualTo(vc.View.SafeAreaLayoutGuide.LeadingAnchor, 24),
				stack.TrailingAnchor.ConstraintEqualTo(vc.View.SafeAreaLayoutGuide.TrailingAnchor, -24),
			});

			Window.RootViewController = vc;
			Window.MakeKeyAndVisible ();
		}
	}

	[Export ("sceneDidDisconnect:")]
	public void DidDisconnect (UIScene scene)
	{
		// Called as the scene is being released by the system.
		// This occurs shortly after the scene enters the background, or when its session is discarded.
		// Release any resources associated with this scene that can be re-created the next time the scene connects.
		// The scene may re-connect later, as its session was not neccessarily discarded (see UIApplicationDelegate `DidDiscardSceneSessions` instead).
	}

	[Export ("sceneDidBecomeActive:")]
	public void DidBecomeActive (UIScene scene)
	{
		// Called when the scene has moved from an inactive state to an active state.
		// Use this method to restart any tasks that were paused (or not yet started) when the scene was inactive.
	}

	[Export ("sceneWillResignActive:")]
	public void WillResignActive (UIScene scene)
	{
		// Called when the scene will move from an active state to an inactive state.
		// This may occur due to temporary interruptions (ex. an incoming phone call).
	}

	[Export ("sceneWillEnterForeground:")]
	public void WillEnterForeground (UIScene scene)
	{
		// Called as the scene transitions from the background to the foreground.
		// Use this method to undo the changes made on entering the background.
	}

	[Export ("sceneDidEnterBackground:")]
	public void DidEnterBackground (UIScene scene)
	{
		// Called as the scene transitions from the foreground to the background.
		// Use this method to save data, release shared resources, and store enough scene-specific state information
		// to restore the scene back to its current state.
	}
}
