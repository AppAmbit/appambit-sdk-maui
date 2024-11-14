
using static Android.Provider.Settings;

namespace Kava.Helpers
{
    public partial class DeviceHelper
    {
        public string GetDeviceId()
        {
            var context = Android.App.Application.Context;

            string? id = Secure.GetString(context.ContentResolver, Secure.AndroidId);

            return id!;
        }
    }
}