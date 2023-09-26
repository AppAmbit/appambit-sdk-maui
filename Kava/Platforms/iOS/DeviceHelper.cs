using System;
using UIKit;

namespace Kava.Helpers
{
	public partial class DeviceHelper
	{
		public partial string GetDeviceId()
		{
			return UIDevice.CurrentDevice.IdentifierForVendor.AsString();
		}

	}
}


