using System.Diagnostics;
using AppAmbit.Models.Breadcrums;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;

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
        var createdAt = DateTime.UtcNow;
        Debug.WriteLine($"BreadcrumbManager: {name} - {createdAt}");
        var entity = new BreadcrumEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = createdAt
        };

        var sent = await TrySendAsync(entity);
        if (!sent && _storage != null)
        {
            await _storage.AddBreadcrumbAsync(entity);
        }
    }

    public static async Task SendPending()
    {
        
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
}
