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
    }
}