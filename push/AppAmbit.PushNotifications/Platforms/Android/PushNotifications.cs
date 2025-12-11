using Android.Content;
using AndroidX.Activity;
using AndroidX.Core.App;
using Android.Util;
using AppAmbit;
using Com.Appambit.Sdk.Models;
using Com.Appambit.Sdk;

namespace AppAmbit.PushNotifications;

/// <summary>
/// Bridge to the AppAmbit Push Notifications AAR plus orchestration with the .NET core SDK.
/// </summary>
public static class PushNotifications
{
    private const string LogTag = "AppAmbitPushSDK";
    private static bool _initialized;
    private static string? _lastPushToken;

    public interface IPermissionListener
    {
        void OnPermissionResult(bool isGranted);
    }

    public interface INotificationCustomizer
    {
        void Customize(Context context, NotificationCompat.Builder builder, AppAmbitNotification notification);
    }

    /// <summary>
    /// Starts the push kernel and wires it to the .NET core SDK to update the consumer.
    /// </summary>
    public static void Start(Context context, bool enableNotifications = true)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var appContext = context.ApplicationContext;

        if (!_initialized)
        {
            PushKernel.SetTokenListener(new TokenListenerProxy(appContext));
            _initialized = true;
        }

        _ = Task.Run(() =>
        {
            try
            {
                PushKernel.Start(appContext);
                PushKernel.SetNotificationsEnabled(appContext, enableNotifications);
            }
            catch (Java.Lang.IllegalStateException ex)
            {
                Log.Error(LogTag, $"Failed to start push: {ex}");
            }
        });
    }

    public static void SetNotificationsEnabled(Context context, bool enabled)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        try
        {
            PushKernel.SetNotificationsEnabled(context.ApplicationContext, enabled);
        }
        catch (Java.Lang.IllegalStateException ex)
        {
            Log.Error(LogTag, $"Failed to set notifications enabled={enabled}: {ex}");
        }

        if (!enabled && !string.IsNullOrWhiteSpace(_lastPushToken))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await AppAmbitSdk.UpdateConsumerAsync(_lastPushToken, false);
                }
                catch (Exception ex)
                {
                    Log.Error(LogTag, $"Failed to sync consumer push state (enabled={enabled}): {ex}");
                }
            });
        }
    }

    public static bool IsNotificationsEnabled(Context context) =>
        PushKernel.AreNotificationsEnabled(context.ApplicationContext);

    public static void RequestNotificationPermission(AndroidX.Activity.ComponentActivity activity) =>
        PushKernel.RequestNotificationPermission(activity, null);

    public static void RequestNotificationPermission(AndroidX.Activity.ComponentActivity activity, IPermissionListener listener) =>
        PushKernel.RequestNotificationPermission(activity, new PermissionListenerProxy(listener));

    public static void SetNotificationCustomizer(INotificationCustomizer? customizer)
    {
        PushKernel.NotificationCustomizer = customizer is null
            ? null
            : new NotificationCustomizerProxy(customizer);
    }

    public static INotificationCustomizer? GetNotificationCustomizer()
    {
        return PushKernel.NotificationCustomizer is NotificationCustomizerProxy proxy
            ? proxy.Managed
            : null;
    }

    private sealed class PermissionListenerProxy : Java.Lang.Object, Com.Appambit.Sdk.PushKernel.IPermissionListener
    {
        private readonly IPermissionListener _managed;

        public PermissionListenerProxy(IPermissionListener managed)
        {
            _managed = managed;
        }

        public void OnPermissionResult(bool isGranted)
        {
            _managed.OnPermissionResult(isGranted);
        }
    }

    private sealed class NotificationCustomizerProxy : Java.Lang.Object, Com.Appambit.Sdk.PushKernel.INotificationCustomizer
    {
        public INotificationCustomizer Managed { get; }

        public NotificationCustomizerProxy(INotificationCustomizer managed)
        {
            Managed = managed;
        }

        public void Customize(Context context, NotificationCompat.Builder builder, AppAmbitNotification notification)
        {
            Managed.Customize(context, builder, notification);
        }
    }

    private sealed class TokenListenerProxy : Java.Lang.Object, Com.Appambit.Sdk.PushKernel.ITokenListener
    {
        private readonly Context _context;

        public TokenListenerProxy(Context context)
        {
            _context = context;
        }

        public void OnNewToken(string token)
        {
            if (!PushKernel.AreNotificationsEnabled(_context))
                return;

            Log.Debug(LogTag, $"FCM token received: {token}");
            _lastPushToken = token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await AppAmbitSdk.UpdateConsumerAsync(token, true);
                }
                catch (Exception ex)
                {
                    Log.Error(LogTag, $"Failed to update consumer with new FCM token: {ex}");
                }
            });
        }
    }
}
