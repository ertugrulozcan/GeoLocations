using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GeoLocations.MMDB.Exceptions;
using GeoLocations.MMDB.Utils;

namespace GeoLocations.MMDB
{
	internal sealed class DictionaryActivatorCreator
	{
		#region Fields

		private readonly ConcurrentDictionary<Type, ObjectActivator> _dictActivators = new();

		#endregion

		#region Methods

		internal ObjectActivator GetActivator(Type expectedType) => _dictActivators.GetOrAdd(expectedType, DictionaryActivator);

		private static ObjectActivator DictionaryActivator(Type expectedType)
		{
			var genericArgs = expectedType.GetGenericArguments();
			if (genericArgs.Length != 2)
				throw new DeserializationException($"Unexpected number of Dictionary generic arguments: {genericArgs.Length}");
			
			ConstructorInfo? constructor;
			if (expectedType.GetTypeInfo().IsInterface)
			{
				var dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
				ReflectionUtil.CheckType(expectedType, dictType);
				constructor = dictType.GetConstructor(new[] { typeof(int) });
			}
			else
			{
				ReflectionUtil.CheckType(typeof(IDictionary), expectedType);
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