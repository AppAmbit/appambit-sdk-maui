namespace AppAmbit.Utils
{
    internal static class FormattedRequestSize
    {
        public static string FormatSize(double size)
        {
            if (size < 1024)
            {
                return $"{size:F2} B";
            }
            else if (size < 1024 * 1024)
            {
                return $"{size / 1024:F2} KB";
            }
            else
            {
                return $"{size / 1024 * 1024:F2} MB";
            }
        }
    }
}
