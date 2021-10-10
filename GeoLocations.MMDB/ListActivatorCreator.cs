using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GeoLocations.MMDB.Utils;
using GeoLocations.MMDB.Exceptions;

namespace GeoLocations.MMDB
{
	internal sealed class ListActivatorCreator
	{
		#region Fields

		private readonly ConcurrentDictionary<Type, ObjectActivator> _listActivators = new();

		#endregion

		#region Methods
		
		internal ObjectActivator GetActivator(Type expectedType) => _listActivators.GetOrAdd(expectedType, ListActivator);

		private static ObjectActivator ListActivator(Type expectedType)
		{
			var genericArgs = expectedType.GetGenericArguments();
			var argType = genericArgs.Length switch
			{
				0 => typeof(object),
				1 => genericArgs[0],
				_ => throw new DeserializationException($"Unexpected number of generic arguments for list: {genericArgs.Length}"),
			};
			
			ConstructorInfo? constructor;
			var interfaceType = typeof(ICollection<>).MakeGenericType(argType);
			var listType = typeof(List<>).MakeGenericType(argType);
			if (expectedType.IsAssignableFrom(listType))
			{
				constructor = listType.GetConstructor(new[] { typeof(int) });
			}
			else
			{
				ReflectionUtil.CheckType(interfaceType, expectedType);
				constructor = expectedType.GetConstructor(Type.EmptyTypes);
			}
			
			if (constructor == null)
				throw new DeserializationException($"Unable to find default constructor for {expectedType}");
			
			var activator = ReflectionUtil.CreateActivator(constructor);
			return activator;
		}

		#endregion
	}
}