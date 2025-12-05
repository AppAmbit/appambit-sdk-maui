using System;
using AppAmbitSdkCore;
namespace AppAmbitSdkAvalonia;

public class AppAmbitSdk
{
    public static void Start(string appKey)
    {
        AppAmbitSdkCore.AppAmbitSdk.Start(appKey);
    }
}