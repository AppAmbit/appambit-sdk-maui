using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kava.Mvvm;

public abstract class BaseViewModel : ObservableObject
{
	private static Dictionary<Type, object[]?> _constructorArgs = new Dictionary<Type, object[]?>();

	[MethodImpl(MethodImplOptions.Synchronized)]
	public static void PrepareArguments(Type viewModelType, object[] arguments)
	{
		if (arguments != null || arguments?.Count() > 0)
			_constructorArgs.Add(viewModelType, arguments);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	internal void ConsumeArguments()
	{
		Type viewModelType = this.GetType();
		if (_constructorArgs.ContainsKey(viewModelType))
		{
			var args = _constructorArgs.GetValueOrDefault(viewModelType);
			_constructorArgs.Remove(viewModelType);

			if (args != null)
			{
				Initialize(args!);
			}
		}
	}

	public BaseViewModel() { }

	public virtual void Initialize(object[] arguments) { }
}

public abstract class BaseViewModel<TParameter> : BaseViewModel where TParameter : class
{
	public override void Initialize(object[] arguments)
	{
		base.Initialize(arguments);
		TParameter param = (TParameter)arguments[0];
		Initialize(param);
	}

	public virtual void Initialize(TParameter parameter) { }
}