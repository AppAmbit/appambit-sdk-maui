using SQLite;

namespace Shared.Models.App;

public class AppSecrets
{
    [PrimaryKey]
    public string? AppId { get; set; }
    
    public string? DeviceId { get; set; } 
    
    public string? Token { get; set; }
    
    public string? SessionId { get; set; }
    
    public string? UserId { get; set; }
    
    public string? UserEmail { get; set; }
}