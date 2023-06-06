using System;
using Android.Nfc;
using Java.Util.Logging;
using Kava.Helpers;

namespace Kava.Logging
{
	public class KavaLogger : ILogService
	{

        readonly static string LOG_FILE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KavaUp", "Log.txt");

		public KavaLogger()
		{
            if (!File.Exists(LOG_FILE_PATH))
                File.Create(LOG_FILE_PATH);
		}

        public async Task<LogEntry[]> GetLogEntries()
        {
            var entries = await FileHelper.GetFileContents(LOG_FILE_PATH);

            if (entries == null)
                return new LogEntry[0];

            return entries.Select(entry => LogEntry.UnParse(entry)).ToArray();
        }

        public DateTime getLastLogTime() => File.GetLastWriteTime(LOG_FILE_PATH);

        public Task Log(string message, LogLevel level = LogLevel.INFO, string tag = "DEFAULT") => Task.Run(async () =>
        {
            await LogAsync(message, level, tag);
        });    

        public async Task<LogEntry> LogAsync(string message, LogLevel level = LogLevel.INFO, string tag = "DEFAULT")
        {
            var entry = new LogEntry
            {
                Message = message,
                LogLevel = level,
                LogTag = tag,
                CreatedAd = DateTime.Now
            };

            await SaveLogToFile(entry);
            return entry;
        }

        async Task SaveLogToFile(LogEntry entry)
        {
            await Task.Run(() =>
            {
                FileHelper.AddTextToFile(entry.Parse(), LOG_FILE_PATH);
            });
        }


    }
}

