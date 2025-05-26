using System.Diagnostics;
using Newtonsoft.Json;

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
    
    public static void AppendToJsonArrayFile<T>(T entry) where T : class
        => AppendToJsonArrayFile(entry, GetFileName(typeof(T)));

    public static void AppendToJsonArrayFile<T>(T entry, string fileName) where T : class
    {
        try
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            var path = GetFilePath(fileName);
            var list = File.Exists(path)
                ? JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path)) ?? new()
                : new();
            list.Add(entry);
            File.WriteAllText(path, JsonConvert.SerializeObject(list, Formatting.Indented));
        }
        catch (Exception e)
        {
            Debug.WriteLine($"File Exception: {e.Message}");
        }
    }    
}