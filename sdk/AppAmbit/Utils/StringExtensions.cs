namespace AppAmbit;

internal static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (maxLength < 0) return value;
        if (value.Length <= maxLength) return value;

        var truncated = value.Substring(0, maxLength);
        return truncated;
    }
    
    public static bool IsUIntNumber(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return ulong.TryParse(value, out _);
    }

}