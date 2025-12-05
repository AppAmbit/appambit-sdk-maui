namespace AppAmbitSdkCore.Services.Interfaces;

public interface IAppInfoService
{
    string? AppVersion { get; set; }
    
    string? Build { get; set; }

    string? Platform { get; set; }
    
    string? OS { get; set; }
    
    string? DeviceModel { get; set; }
    
    string? Country { get; set; }
    
    string? UtcOffset { get; set; }
    
    string? Language { get; set; }
}
