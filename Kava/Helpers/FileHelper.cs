using System;
using Kava.Logging;

namespace Kava.Helpers
{
	public static class FileHelper
	{
		static object lockObject = new object();

		public static void AddTextToFile (string text, String path)
        {
			lock (lockObject)
			{
				FileStream fileStream;

				if (File.Exists(path))
					fileStream = File.OpenWrite(path);
				else
					fileStream = File.Create(path);

				File.AppendAllLines(path, new String[1] { text });
			}
		} 

		public static async Task<string[]> GetFileContents(String path) => await File.ReadAllLinesAsync(path);

	}
}

