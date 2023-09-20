using System.Reflection;

namespace Kava.Helpers;

public static class ReflectionHelper
{

	public static List<Type>? GetTypesFrom<T>(Assembly assembly) where T : class
		=> assembly
			.GetTypes()
			.Where(p => p.GetTypeInfo().IsSubclassOf(typeof(T)))
			.ToList();

	public static T? CreateInstanceOf<T>(params object[] constructorArgs) where T : class
		=> Activator.CreateInstance(typeof(T), constructorArgs) as T;
}