namespace Kava.Helpers;

public static class FileHelper
{
	static object lockObject = new object();

	public static bool CreateFileWithDirectory(string dataPath, string directory, string fileName)
	{
		lock (lockObject)
		{
			var fullPath = Path.Combine(dataPath, directory, fileName);

			try
			{
				if (!File.Exists(fullPath))
				{
					var directoryPath = Path.Combine(dataPath, directory);
					if (!Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}

					File.Create(fullPath);

					return true;
				}
				else
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}

	public static void AddTextToFile(string text, string path)
	{
		lock (lockObject)
		{
			FileStream fileStream;

			if (!File.Exists(path))
				fileStream = File.Create(path);

			using (StreamWriter sr = File.AppendText(path))
			{
				sr.WriteLine(text);
			}
		}
	}

	public static string[] GetFileContents(String path)
	{
		lock (lockObject)
		{
			return File.ReadAllLines(path); 
		}
	} 

	public static bool ClearLog(string dataPath, string directory)
	{
		try
		{
			lock (lockObject)
			{
				var fullPath = Path.Combine(dataPath, directory);
				File.Delete(fullPath);
			}
			
			return true;
		}
		catch
		{
			return false;
		}
	}
	
	public static bool ClearLog(string dataPath, string directory, string fileName)
	{
		try
		{
			lock (lockObject)
			{
				var fullPath = Path.Combine(dataPath, directory, fileName);
				File.Delete(fullPath);
			}
			return CreateFileWithDirectory(dataPath, directory, fileName);
		}
		catch
		{
			return false;
		}
	}

	public static long GetFileSize(string path)
	{
		try
		{
			return new FileInfo(path).Length;
		}
		catch
		{
			return -1;
		}
	}

}