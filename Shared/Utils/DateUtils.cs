namespace Shared.Utils;

public static class DateUtils
{
    static public DateTime GetUtcNow { get { return DateTime.UtcNow; } }
    static public string GetUtcNowFormatted { get { return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); } }
    
}