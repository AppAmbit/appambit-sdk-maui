using System.Diagnostics;
using Newtonsoft.Json;
using AppAmbit.Utils;
using Newtonsoft.Json.Converters;
namespace AppAmbit;

internal static class FileUtils
{
    internal static async Task<T?> GetSavedSingleObject<T>() where T : class
    {
        try
        {
            var filePath = GetFilePath(GetFileName(typeof(T)));
            Debug.WriteLine($"filePath {typeof(T).Name}: {filePath}");
            if (File.Exists(filePath))
            {
                var fileText = await File.ReadAllTextAsync(filePath);
                File.Delete(filePath);
                return JsonConvert.DeserializeObject<T>(fileText);
            }
            return null as T;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"File Exception: {e.Message}");
            return null as T;
        }
    }
    internal static string GetFilePath(string fileName)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
        return path;
    }

    internal static string GetFileName(Type type)
    {
        var fileName = $"{type.Name}.json";
        return fileName;
    }

    internal static void SaveToFile<T>(string json) where T : class
    {
        var filePath = GetFilePath(GetFileName(typeof(T)));
        File.WriteAllText(filePath, json);
        Debug.WriteLine($"Saved {typeof(T).Name} to {filePath}");
    }


    public static async Task<List<T>> GetSaveJsonArrayAsync<T>(string fileName, T? entry)
        where T : class, IIdentifiable
    {
        try
        {
            fileName = PrepareFileSettings(fileName, out JsonSerializerSettings settings, out string path);

            var list = File.Exists(path)
                ? JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path), settings) ?? []
                : [];

            if (entry is not null && !list.Any(x => x.Id == entry.Id))
            {
                list.Add(entry);
                var listSorted = list.OrderBy(x => x.Timestamp).ToList();
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(listSorted, settings));
            }

            return list;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"File Exception: {e.Message}");
            return [];
        }
    }

    internal static async Task UpdateJsonArrayAsync<T>(string fileName, IEnumerable<T> updatedList)
    {
        try
        {
            fileName = PrepareFileSettings(fileName, out JsonSerializerSettings settings, out string path);

            if (updatedList.Count() == 0)
            {
                File.Delete(path);
                return;
            }

            var json = JsonConvert.SerializeObject(updatedList, settings);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception)
        {
            Debug.WriteLine("Error to save file json");
        }
    }    

    private static string PrepareFileSettings(string fileName, out JsonSerializerSettings settings, out string path)
    {
        settings = new JsonSerializerSettings
        {
            Converters = [new StringEnumConverter()],
            Formatting = Formatting.Indented
        };
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fileName += ".json";

        path = GetFilePath(fileName);
        return fileName;
    }
}