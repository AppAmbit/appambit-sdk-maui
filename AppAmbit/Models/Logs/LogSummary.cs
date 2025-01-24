using Newtonsoft.Json;

namespace AppAmbit.Models.Logs;

public class LogSummary
{
    [JsonProperty("title")]
    public string? Title { get; set; }
    
    [JsonProperty("app_version")]
    public string? AppVersion { get; set; }
    
    [JsonProperty("country_iso")]
    public string? CountryISO { get; set; }
    
    [JsonProperty("device_id")]
    public string? DeviceId { get; set; }
    
    [JsonProperty("device_model")]
    public string? DeviceModel { get; set; }

    [JsonProperty("platform")]
    public string? Platform { get; set; }
    
    [JsonProperty("error_count")]
    public int ErrorCount { get; set; }
    
    [JsonProperty("crash_count")]
    public int CrashCount { get; set; }
    
    [JsonProperty("info_count")]
    public int InformationCount { get; set; }
    
    [JsonProperty("debug_count")]
    public int DebugCount { get; set; }
    
    [JsonProperty("warn_count")]
    public int WarningCount { get; set; }
    
    [JsonProperty("groups")]
    public List<LogGrouping> Groups { get; set; }
}