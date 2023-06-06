using System;
using System.Diagnostics.CodeAnalysis;
using Kava.Helpers;

namespace Kava.Logging
{
	public class LogEntry : IParseable<LogEntry>
    {
        const string identifier = ",";
		public LogLevel LogLevel { get; set; }
		public DateTime CreatedAd { get; set; }
		public string? LogTag { get; set; }
		public string? Message { get; set; }

		public new string ToString => $"[{LogLevel.convertToString()}] [{LogLevel}] {CreatedAd.ToLongTimeString}: {Message}";

        public static string? UnsanitizedMessage(string text) => text?.Replace(identifier, "c0mm4");

        public static string SanitizedText(string text) => text.Replace("c0mm4", identifier);

        public string Parse()
        {
            return $"{LogLevel.convertToString()}{identifier}{LogTag}{identifier}{Message}{identifier}{CreatedAd.ToFileTimeUtc};";
        }

        public static LogEntry UnParse(string parsedFormat)
        {
            String[] values = parsedFormat.Replace(";", String.Empty).Split(identifier);
            return new LogEntry
            {
                LogLevel = values[0].converToEnum<LogLevel>(),
                LogTag = SanitizedText(values[1]),
                Message = SanitizedText(values[2]),
                CreatedAd = Convert.ToDateTime(values[3])
            };  
        }
    }
}

