using System;
using System.Linq.Expressions;
using System.Reflection;
using GeoLocations.MMDB.Exceptions;

namespace GeoLocations.MMDB.Utils
{
	#region Delegations

	internal delegate object ObjectActivator(params object[] args);

	#endregion

	internal static class ReflectionUtil
	{
		#region Methods

		internal static ObjectActivator CreateActivator(ConstructorInfo constructor)
		{
			if (constructor == null)
			{
				throw new ArgumentNullException(nameof(constructor));
			}
			var paramInfo = constructor.GetParameters();

			var paramExp = Expression.Parameter(typeof(object[]), "args");

			var argsExp = new Expression[paramInfo.Length];
			for (var i = 0; i < paramInfo.Length; i++)
			{
				var index = Expression.Constant(i);
				var paramType = paramInfo[i].ParameterType;
				var accessorExp = Expression.ArrayIndex(paramExp, index);
				var castExp = Expression.Convert(accessorExp, paramType);
				argsExp[i] = castExp;
			}

			var newExp = Expression.New(constructor, argsExp);
			var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, paramExp);
			return (ObjectActivator)lambda.Compile();
		}

		internal static void CheckType(Type expected, Type from)
		{
			if (!expected.IsAssignableFrom(from))
			{
				throw new DeserializationException($"Could not convert '{from}' to '{expected}'.");
			}
		}

		#endregion
	}
}