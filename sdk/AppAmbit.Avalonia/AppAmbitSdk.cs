namespace AppAmbitAvalonia;

public static partial class AppAmbitSdk
{
    public static void Start(string appKey)
    {
        AppAmbit.AppAmbitSdk.Start(appKey);

        if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            PlatformStartHooks();
        }
    }
    static partial void PlatformStartHooks();
}