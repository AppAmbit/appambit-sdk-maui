namespace Kava.Mvvm;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class RegisterRouteAttribute : Attribute
{
	private string _route;
	private Type _pageType;

	public RegisterRouteAttribute(string route, Type pageType)
	{
		_route = route;
		_pageType = pageType;
	}

	public string Route => _route;

	public Type PageType => _pageType;
}