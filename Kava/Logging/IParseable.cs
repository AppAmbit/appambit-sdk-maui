using System;
namespace Kava.Logging
{
	public interface IParseable<T> where T : class
	{
		string Parse();
		static T? UnParse(string parsedFormat) => null;
	}
}

