using System.Globalization;
using AppAmbit.Services.Interfaces;
#if ANDROID
using Android.Provider;
#elif IOS
using UIKit;
#endif

namespace AppAmbit.Services;

internal class AppInfoService : IAppInfoService
{
    public AppInfoService()
    {
        AppVersion = AppInfo.Current.VersionString;
        Build = AppInfo.Current.BuildString;
        Platform = DeviceInfo.Current.Platform.ToString();
        OS = DeviceInfo.Current.Version.ToString();
        DeviceModel = DeviceInfo.Current.Model; 
        Country = RegionInfo.CurrentRegion.Name;
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