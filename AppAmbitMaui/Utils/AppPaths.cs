namespace AppAmbit
{
    public static class AppPaths
    {
        public static string AppDataDir
        {
            get
            {
                var p = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(p))
                    p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return p;
            }
        }
    }
}
