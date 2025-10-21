namespace AppAmbit;

internal static class BreadcrumbManager
{
    private static IAPIService? _api;
    private static IStorageService? _storage;
    private static readonly SemaphoreSlim _sendGate = new(1, 1);

    public static void Initialize(IAPIService api, IStorageService storage)
    {
        _api = api;
        _storage = storage;
    }

    public static async Task AddAsync(string name)
    {
        var entity = new BreadcrumEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = UtcNowSeconds()
        };

        var sent = await TrySendAsync(entity);
        if (!sent && _storage != null)
        {
            await _storage.AddBreadcrumbAsync(entity);
        }
    }

    private static async Task<bool> TrySendAsync(BreadcrumEntity entity)
    {
        if (_api == null) return false;
        try
        {
            var ep = new BreadcrumbEndpoint(entity);
            await _api.ExecuteRequest<Response>(ep);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static DateTime UtcNowSeconds()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
    }
}
