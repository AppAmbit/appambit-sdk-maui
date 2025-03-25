using System.Globalization;
using System.Runtime.InteropServices;
using iOSAppAmbit.Services.Base;

namespace iOSAppAmbit.Services;

internal class AppInfoService : IAppInfoService
{
    public AppInfoService()
    {
        AppVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString();
        Platform = "iOS";
        OS = RuntimeInformation.OSDescription;;
        DeviceModel = UIDevice.CurrentDevice.Model;
        Country = RegionInfo.CurrentRegion.Name;
        UtcOffset = TimeZoneInfo.Local.BaseUtcOffset.ToString();
        Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }
    
    public string? AppVersion { get; set; }
    
    public string? Platform { get; set; }
    
    public string? OS { get; set; }

    public string? DeviceModel { get; set; }
    
    public string? Country { get; set; }
    
    public string? UtcOffset { get; set; }
    
    public string? Language { get; set; }
}