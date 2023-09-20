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

	public new string ToString => $"[{LogLevel.convertToString()}] [{LogLevel}] {CreatedAt.ToLongTimeString}: {Message}";

	public static string? UnsanitizedMessage(string text) => text?.Replace(identifier, "c0mm4");

	public static string SanitizedText(string text) => text.Replace("c0mm4", identifier);

	public string Parse()
	{
		return $"{LogLevel.convertToString()}{identifier}{LogTag}{identifier}{Message}{identifier}{CreatedAt.ToString()};";
	}

	public static LogEntry UnParse(string parsedFormat)
	{
		String[] values = parsedFormat.Replace(";", String.Empty).Split(identifier);
		return new LogEntry
		{
			LogLevel = values[0].converToEnum<LogLevel>(),
			LogTag = SanitizedText(values[1]),
			Message = SanitizedText(values[2]),
			CreatedAt = Convert.ToDateTime(values[3])
		};  
	}
}