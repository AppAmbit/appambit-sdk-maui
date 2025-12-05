using System.Globalization;
using AppAmbitSdkCore.Services.Interfaces;
using System.Reflection;
using System.Runtime.InteropServices;

#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using AOSBuild = Android.OS.Build;
#elif IOS && !MACCATALYST
using Foundation;
using UIKit;
#elif MACCATALYST
using Foundation;
using UIKit;
#elif WINDOWS
using Microsoft.Win32;
#endif

namespace AppAmbitSdkCore.Services;

internal class AppInfoService : IAppInfoService
{
    public AppInfoService()
    {
#if ANDROID
        Platform = "Android";
        OS = AOSBuild.VERSION.Release ?? ((int)AOSBuild.VERSION.SdkInt).ToString();
        DeviceModel = AOSBuild.Model;

        var context = global::Android.App.Application.Context;
        var pm = context.PackageManager;
        var packageName = context.PackageName;

        PackageInfo? pInfo;
#pragma warning disable 612, 618
        pInfo = pm?.GetPackageInfo(packageName!, 0);
#pragma warning restore 612, 618

        AppVersion = pInfo?.VersionName;
        long code = 0;
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            code = (long)(pInfo?.LongVersionCode ?? 0);
        }
        else
        {
#pragma warning disable 612, 618
            code = pInfo?.VersionCode ?? 0;
#pragma warning restore 612, 618
        }
        Build = code.ToString();
#elif IOS && !MACCATALYST
        Platform = "iOS";
        OS = UIDevice.CurrentDevice.SystemVersion;
        DeviceModel = UIDevice.CurrentDevice.Model;

        var bundle = NSBundle.MainBundle;
        AppVersion = bundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString();
        Build = bundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString();

#elif WINDOWS
        Platform = "Windows";
        OS = RuntimeInformation.OSDescription;
        DeviceModel = Environment.MachineName;

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        AppVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();

        Build = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#elif MACCATALYST
        Platform = "macOS";
        OS = NSProcessInfo.ProcessInfo.OperatingSystemVersionString;
        DeviceModel = "Mac";
        var bundle = NSBundle.MainBundle;
        AppVersion = bundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString();
        Build = bundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString();
#else
        Platform = RuntimeInformation.OSDescription;
        OS = Environment.OSVersion.VersionString;
        DeviceModel = Environment.MachineName;
        AppVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
        Build = Assembly.GetEntryAssembly()?.GetName()?.Version?.Build.ToString();
#endif

        Country = RegionInfo.CurrentRegion?.Name;
        UtcOffset = TimeZoneInfo.Local.BaseUtcOffset.ToString();
        Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public string? AppVersion { get; set; }
    public string? Build { get; set; }
    public string? Platform { get; set; }
    public string? OS { get; set; }
    public string? DeviceModel { get; set; }
    public string? Country { get; set; }
    public string? UtcOffset { get; set; }
    public string? Language { get; set; }
}
