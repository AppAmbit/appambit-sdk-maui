using Newtonsoft.Json;

namespace AppAmbit;

public static class ObjectExtensions
{
    public static T ConvertTo<T>(this object source) where T : class, new()
    {
        if (source == null) return null;
        
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore // Ignore unknown properties
        };

        // Serialize the source object to JSON and deserialize it into the target type
        string json = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(json,jsonSerializerSettings) ?? new T();
    }
}