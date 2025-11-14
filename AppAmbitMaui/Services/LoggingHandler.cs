
using System.Diagnostics;

namespace AppAmbit.Services;

public class LoggingHandler : DelegatingHandler
{
    private static double _totalRequestSize = 0;
    private static readonly object _lock = new object();

    public static double TotalRequestSize
    {
        get
        {
            lock (_lock)
            {
                return _totalRequestSize;
            }
        }
    }

    public LoggingHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var reqBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Debug.WriteLine(reqBody);
        }

        var response = await base.SendAsync(request, cancellationToken);
        await CalculateRequestSize(request);

        if (response.Content != null)
        {
            var respBody = await response.Content.ReadAsStringAsync(cancellationToken);
            Debug.WriteLine(respBody);
        }
        return response;
    }

    internal static async Task CalculateRequestSize(HttpRequestMessage request)
    {
        int bodySize = 0;

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            bodySize = content.Length;
        }

        lock (_lock)
        {
            _totalRequestSize += bodySize;
        }

        Debug.WriteLine($"[APIService] - Request Size: {bodySize} bytes, Total: {_totalRequestSize:F4} bytes");
    }

    public static void ResetTotalSize()
    {
        lock (_lock)
        {
            _totalRequestSize = 0;
        }
    }
}