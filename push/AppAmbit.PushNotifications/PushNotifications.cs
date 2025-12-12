using System;
#if ANDROID
using Android.Content;
using AndroidX.Core.App;
using Com.Appambit.Sdk.Models;
using ActivityBase = AndroidX.Activity.ComponentActivity;
#endif

namespace AppAmbit.PushNotifications;

/// <summary>
/// Cross-platform facade for AppAmbit Push.
/// </summary>
public static class PushNotifications
{
    internal const string LogTag = "AppAmbitPushSDK";

    public interface IPermissionListener
    {
        void OnPermissionResult(bool isGranted);
    }

#if ANDROID
    public interface INotificationCustomizer
    {
        void Customize(Context context, NotificationCompat.Builder builder, AppAmbitNotification notification);
    }

    public static void Start(Context context, bool enableNotifications = true) =>
        PushNotificationsAndroid.Start(context, enableNotifications);

    public static void SetNotificationsEnabled(Context context, bool enabled) =>
        PushNotificationsAndroid.SetNotificationsEnabled(context, enabled);

    public static bool IsNotificationsEnabled(Context context) =>
        PushNotificationsAndroid.IsNotificationsEnabled(context);

    public static void RequestNotificationPermission(ActivityBase activity) =>
        PushNotificationsAndroid.RequestNotificationPermission(activity, null);

    public static void RequestNotificationPermission(ActivityBase activity, IPermissionListener? listener) =>
        PushNotificationsAndroid.RequestNotificationPermission(activity, listener);

    public static void SetNotificationCustomizer(INotificationCustomizer? customizer) =>
        PushNotificationsAndroid.SetNotificationCustomizer(customizer);

    public static INotificationCustomizer? GetNotificationCustomizer() =>
        PushNotificationsAndroid.GetNotificationCustomizer();
#else
    // Future platforms: provide implementations here. For now, throw to indicate unsupported.
    public static void Start(object context, bool enableNotifications = true) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static void SetNotificationsEnabled(object context, bool enabled) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static bool IsNotificationsEnabled(object context) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static void RequestNotificationPermission(object activity) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static void RequestNotificationPermission(object activity, IPermissionListener? listener) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static void SetNotificationCustomizer(INotificationCustomizer? customizer) =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
    public static INotificationCustomizer? GetNotificationCustomizer() =>
        throw new PlatformNotSupportedException("PushNotifications is not supported on this platform.");
#endif
}
