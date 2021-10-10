using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeoLocations.MMDB.Utils;

namespace GeoLocations.MMDB
{
	internal readonly struct TypeActivator
	{
		#region Fields

		internal readonly ObjectActivator Activator;
		internal readonly List<ParameterInfo> AlwaysCreatedParameters;
		internal readonly object?[] _defaultParameters;
		internal readonly Dictionary<Key, ParameterInfo> DeserializationParameters;
		internal readonly Dictionary<string, ParameterInfo> InjectableParameters;
		internal readonly List<ParameterInfo> NetworkParameters;
		internal readonly Type[] ParameterTypes;

		#endregion
		
		#region Constructors
		
		internal TypeActivator(
			ObjectActivator activator,
			Dictionary<Key, ParameterInfo> deserializationParameters,
			Dictionary<string, ParameterInfo> injectables,
			List<ParameterInfo> networkParameters,
			List<ParameterInfo> alwaysCreatedParameters) : this()
		{
			this.Activator = activator;
			this.AlwaysCreatedParameters = alwaysCreatedParameters;
			this.DeserializationParameters = deserializationParameters;
			this.InjectableParameters = injectables;
			this.NetworkParameters = networkParameters;
			
			this.ParameterTypes = deserializationParameters.Values.OrderBy(x => x.Position).Select(x => x.ParameterType).ToArray();
			this._defaultParameters = ParameterTypes.Select(DefaultValue).ToArray();
		}

		#endregion

		#region Methods

		internal object[] DefaultParameters() => (object[]) this._defaultParameters.Clone();

		private static object? DefaultValue(Type type)
		{
			if (type.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(type) == null)
			{
				return System.Activator.CreateInstance(type);
			}
			
			return null;
		}

		#endregion
	}
}