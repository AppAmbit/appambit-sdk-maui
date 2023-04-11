using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace KavaupMaui.Helpers;

public static class DeepCopy
{
  public static T CloneJson<T>(this T source)
  {
    if (ReferenceEquals(source, null)) return default(T);
    var deserializeSettings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};
    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
  }
}
