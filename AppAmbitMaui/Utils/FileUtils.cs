using System.Diagnostics;
using Newtonsoft.Json;

namespace AppAmbit;

internal static class FileUtils
{
    internal static async Task<T?> GetSavedSingleObject<T>() where T : class
    {
        try
        {
            Debug.WriteLine($"AppDataDirectory: {FileSystem.AppDataDirectory}");
            var filePath = GetFilePath(GetFileName(typeof(T)));
            var fileText = await File.ReadAllTextAsync(filePath);
            File.Delete(filePath);
            return JsonConvert.DeserializeObject<T>(fileText);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return null as T;
        }
    }
    internal static string GetFilePath(string fileName)
    {
        Debug.WriteLine($"AppDataDirectory: {FileSystem.AppDataDirectory}");
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