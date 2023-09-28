using System.Text.Json.Nodes;
using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging;

public class LogEntry : IParseable<LogEntry>
{
	const string identifier = ",";
	public LogLevel LogLevel { get; set; }
	public DateTime CreatedAt { get; set; }
	public string? LogTag { get; set; }
	public string? Message { get; set; }

	public JsonObject Data { get; set; } = new JsonObject();

	public new string ToString => $"[{LogLevel.convertToString()}] [{LogLevel}] {CreatedAt.ToLongTimeString}: {Message}";

	public static string? UnsanitizedMessage(string text) => text?.Replace(identifier, "c0mm4");

	public static string SanitizedText(string text) => text.Replace("c0mm4", identifier);

	public string Parse()
	{
		return $"{LogLevel.convertToString()}{CreatedAt.ToString()}{identifier}{LogTag}{identifier}{Message}{identifier}{convertJsonToText(Data)};";
	}

	public static LogEntry UnParse(string parsedFormat)
	{
		String[] values = parsedFormat.Replace(";", String.Empty).Split(identifier);
		return new LogEntry
		{
			LogLevel = values[0].converToEnum<LogLevel>(),
			CreatedAt = Convert.ToDateTime(values[1]),
			LogTag = SanitizedText(values[2]),
			Message = SanitizedText(values[3]),
			Data = convertTextToJson(values[5])
		};  
	}
	
	// convert json object to text
	private static string convertJsonToText(JsonObject jsonObject)
	{
		return jsonObject.ToString();
	}
	
	// convert text to Json object
	private static JsonObject convertTextToJson(string text)
	{
		return JsonObject.Parse(text)?.AsObject() ?? new JsonObject();
	}
}