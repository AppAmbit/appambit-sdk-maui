using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace AppAmbit.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SessionType
{
    [EnumMember(Value = "Start")]
    Start,
    
    [EnumMember(Value = "End")]
    End
}
