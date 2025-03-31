using Newtonsoft.Json;

namespace AppAmbit;

public static class ObjectExtensions
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
}