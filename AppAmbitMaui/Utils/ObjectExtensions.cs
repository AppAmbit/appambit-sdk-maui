using System.Collections;
using Newtonsoft.Json;

namespace AppAmbit;

internal static class ObjectExtensions
{
    public static T ConvertTo<T>(this object source) where T : class, new()
    {
        if (source == null) return null;

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        try
        {
            string json = JsonConvert.SerializeObject(source);
            var result = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            return result ?? throw new InvalidOperationException("Deserialization failed for object.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to convert object", ex);
        }
    }

    public static bool IsCollection(this object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    public static bool IsList(this object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        return typeof(System.Collections.IList).IsAssignableFrom(type);
    }

    public static List<object> ToObjectList(this object collection)
    {
        if (collection is not IEnumerable enumerable || collection is string)
            return new List<object>() { collection };

        var list = new List<object>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }

        return list;
    }
}