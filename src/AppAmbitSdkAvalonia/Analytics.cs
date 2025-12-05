using System;

namespace AppAmbitSdkAvalonia;

public static class Analytics
{
    public static void EnableManualSession()
    {
        AppAmbitSdkCore.Analytics.EnableManualSession();
    }

    public static async Task StartSession()
    {
        await AppAmbitSdkCore.Analytics.StartSession();
    }

    public static async Task EndSession()
    {
        await AppAmbitSdkCore.Analytics.EndSession();
    }

    public static async void SetUserId(string userId)
    {
        AppAmbitSdkCore.Analytics.SetUserId(userId);
    }

    public static async Task<string?> GetUserId()
    {
        return await AppAmbitSdkCore.Analytics.GetUserId();
    }

    public static async void SetUserEmail(string userEmail)
    {
        AppAmbitSdkCore.Analytics.SetUserEmail(userEmail);
    }

    public static async Task<string?> GetUserEmail()
    {
        return await AppAmbitSdkCore.Analytics.GetUserEmail();
    }

    public static async Task GenerateTestEvent()
    {
        AppAmbitSdkCore.Analytics.GenerateTestEvent();
    }

    public static async Task TrackEvent(string eventName, 
            Dictionary<string, string>? properties = null)
    {
        await AppAmbitSdkCore.Analytics.TrackEvent(eventName, properties);
    }

    public static void ClearToken()
    {
        AppAmbitSdkCore.Analytics.ClearToken();
    }

}