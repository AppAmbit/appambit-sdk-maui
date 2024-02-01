using System;
using Serilog;
using Serilog.Events;
using System.Globalization;
using Kava.Helpers;
using System.Text;

namespace Kava.Logging.Factory
{
	public static class SeriloggerFactory
	{
		public static void GenerateLogger()
		{
            //if (Serilog.Log.Logger == null)
            //{
                Serilog.Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File(LogHelper.GetLogFilePath(), fileSizeLimitBytes: LogManager.DefaultLogSizeMb)
                    .WriteTo.AmazonS3(
                        "log.txt",
                        "appambit",
                        Amazon.RegionEndpoint.USEast1,
                        "AKIAYA3XVPI3BS6GDMWJ",
                        "6sLau3FgBfSJ+XNfXygExAGgB4EcspnHFVamtENi",
                        restrictedToMinimumLevel: LogEventLevel.Verbose,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        new CultureInfo("de-DE"),
                        levelSwitch: null,
                        rollingInterval: Serilog.Sinks.AmazonS3.RollingInterval.Minute,
                        encoding: Encoding.Unicode,
                        failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}")
                    )
                    .CreateLogger();
            //} else
            //{
                
           // }
        }
	}
}

