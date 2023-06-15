using System;

namespace Kava.Logging
{
	public class MockNetworkLogService : INetworkLogService
	{
		public MockNetworkLogService()
		{
		}

        public Task UploadLogEntries(LogEntry[] entries)
        {
            Task.Run(() => { });
            return null;
            //do nothing
            //throw new NotImplementedException();
        }
    }
}

