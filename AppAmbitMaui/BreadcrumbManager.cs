using System.Diagnostics;
using AppAmbit.Enums;
using AppAmbit.Models.Breadcrumbs;
using AppAmbit.Models.Breadcrums;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using static AppAmbit.FileUtils;

namespace AppAmbit;

internal static class BreadcrumbManager
{
    private static IAPIService? _api;
    private static IStorageService? _storage;
    private static readonly object _lastLock = new();
    private static string? _lastBreadcrumb;

    public static void Initialize(IAPIService api, IStorageService storage)
    {
        _api = api;
        _storage = storage;
    }

    public static async Task AddAsync(string name)
    {
        lock (_lastLock)
        {
            if (_lastBreadcrumb == name) return;
            _lastBreadcrumb = name;
        }

        var entity = CreateBreadcrumb(name);
        await SendBreadcumbs(entity);
    }

    public static void SaveFile(string name)
    {
        lock (_lastLock)
        {
            _lastBreadcrumb = name;
        }

        var breadcrumb = CreateBreadcrumb(name);
        var data = breadcrumb.ToData(sessionId: SessionManager.SessionId);
        GetSaveJsonArray(BreadcrumbsConstants.nameFile, data);
    }

    public static void LoadBreadcrumbsFromFile()
    {
        try
        {
            var files = GetSaveJsonArray<BreadcrumbData>(BreadcrumbsConstants.nameFile, null);
            var notSent = new List<BreadcrumbData>();

            if (files == null || files.Count == 0)
            {
                return;
            }

            foreach (var item in files)
            {
                if (item == null) continue;

                try
                {
                    AsyncHelpers.RunSync(() => _storage!.AddBreadcrumbAsync(item.ToEntity()));
                }
                catch
                {
                    notSent.Add(item);
                }
            }

            UpdateJsonArray(BreadcrumbsConstants.nameFile, notSent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    internal static BreadcrumbsEntity CreateBreadcrumb(string name)
    {
        return new BreadcrumbsEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            SessionId = SessionManager.SessionId ?? "",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static async Task SendBatchBreadcrumbs()
    {
        try
        {
            var items = await _storage.GetOldest100BreadcrumbsAsync();

            if (items == null || items.Count == 0)
            {
                Debug.WriteLine("There are no breadcrumbs");
                return;
            }

            var itemsData = items
                .Select(b => b.ToData(sessionId: b.SessionId ?? string.Empty))
                .ToList();

            var endpoint = new BreadcrumbsBatchEndpoint(itemsData ?? []);

            var responseBatch = await _api.ExecuteRequest<Response>(endpoint);

            if (responseBatch?.ErrorType == ApiErrorType.NetworkUnavailable)
            {
                Debug.WriteLine("Batch of unsent events");
                return;
            }

            await _storage.DeleteBreadcrumbs(items);
            Debug.WriteLine("Finished Breacrumbs Batch");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SendBatchBreadcrumbs error: " + ex);
        }
    }

    private static async Task<bool> TrySendAsync(BreadcrumbsEntity entity)
    {
        if (_api == null) return false;
        try
        {
            var ep = new BreadcrumbEndpoint(entity);
            var result = await _api.ExecuteRequest<Response>(ep);

            if (result.ErrorType == ApiErrorType.None)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static async Task SendBreadcumbs(BreadcrumbsEntity entity)
    {
        var sent = await TrySendAsync(entity);
        if (!sent && _storage != null)
        {
            await _storage.AddBreadcrumbAsync(entity);
        }
    }
}
