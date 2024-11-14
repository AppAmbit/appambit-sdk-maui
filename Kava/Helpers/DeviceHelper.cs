namespace Kava.Helpers
{
    public partial class DeviceHelper
    {
#if (!ANDROID && !IOS && !MACCATALYST && !WINDOWS && !TIZEN)

    public string GetDeviceId() => throw new NotImplementedException();
#endif

    }
}