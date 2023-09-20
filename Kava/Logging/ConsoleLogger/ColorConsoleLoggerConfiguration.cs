using Microsoft.Extensions.Logging;

namespace Kava.Logging.ConsoleLogger;

public sealed class ColorConsoleLoggerConfiguration
{
	public int EventId { get; set; }

	public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
	{
		[LogLevel.Information] = ConsoleColor.Green,
		[LogLevel.Debug] = ConsoleColor.Black,
		[LogLevel.None] = ConsoleColor.Gray,
		[LogLevel.Critical] = ConsoleColor.DarkYellow,
		[LogLevel.Warning] = ConsoleColor.Yellow,
		[LogLevel.Error] = ConsoleColor.Red
	};
}