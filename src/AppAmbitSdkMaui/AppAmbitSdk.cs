using System;
using AppAmbitSdkCore;

namespace AppAmbitSdkMaui;

public static class AppAmbitSdk
{
    public static void Start(string appKey)
    {
        AppAmbitSdkCore.AppAmbitSdk.Start(appKey);
    }

}
