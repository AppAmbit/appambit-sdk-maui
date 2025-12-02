namespace AppAmbit;

internal static class DateUtils
{
    static public string GetUtcNowFormatted { get { return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); } }
}