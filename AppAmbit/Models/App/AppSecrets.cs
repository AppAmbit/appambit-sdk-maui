using SQLite;

namespace AppAmbit.Models.App;

public class AppSecrets
{
    [PrimaryKey]
    public string? AppId { get; set; }
    
    public string? Token { get; set; }
    
    public string? SessionId { get; set; }
}