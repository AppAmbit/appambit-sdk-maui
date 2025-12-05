using System.Threading.Tasks;
#if ANDROID
using Android.Content;
using Android.Net;
#elif IOS
using System.Net;
using SystemConfiguration;
#endif

namespace AppAmbitSdkCore
{
    internal static class NetConnectivity
    {
        public static Task<bool> HasInternetAsync(int timeoutMs = 1500)
        {
#if ANDROID
            try
            {
                var cm = (ConnectivityManager?)global::Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
                if (cm == null) return Task.FromResult(true);

                if (OperatingSystem.IsAndroidVersionAtLeast(23))
                {
                    var net = cm.ActiveNetwork;
                    if (net == null) return Task.FromResult(false);
                    var caps = cm.GetNetworkCapabilities(net);
                    if (caps == null) return Task.FromResult(false);

                    var hasTransport =
                        caps.HasTransport(TransportType.Wifi) ||
                        caps.HasTransport(TransportType.Cellular) ||
                        caps.HasTransport(TransportType.Ethernet) ||
                        caps.HasTransport(TransportType.Bluetooth);

                    return Task.FromResult(hasTransport);
                }
                else
                {
#pragma warning disable 618
                    var info = cm.ActiveNetworkInfo;
                    return Task.FromResult(info != null && info.IsConnectedOrConnecting);
#pragma warning restore 618
                }
            }
            catch
            {
                return Task.FromResult(true);
            }
#elif IOS
            try
            {
                using var reach = new NetworkReachability(new IPAddress(0));
                if (reach.TryGetFlags(out var flags))
                    return Task.FromResult(IsReachable(flags));
            }
            catch { }
            return Task.FromResult(true);
#else
            return Task.FromResult(true);
#endif
        }

#if IOS
        private static bool IsReachable(NetworkReachabilityFlags flags)
        {
            var reachable = (flags & NetworkReachabilityFlags.Reachable) != 0;
            var noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;
            var canConnectAutomatically =
                (flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ||
                (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0;
            var canConnectWithoutUser =
                canConnectAutomatically && (flags & NetworkReachabilityFlags.InterventionRequired) == 0;

            return reachable && (noConnectionRequired || canConnectWithoutUser);
        }
#endif
    }
}
