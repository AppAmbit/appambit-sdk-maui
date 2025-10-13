
namespace AppAmbitTestingiOS;

public static class NetworkMonitor
{
    public static bool IsConnected()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var resp = client.GetAsync("https://www.apple.com").GetAwaiter().GetResult();
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
