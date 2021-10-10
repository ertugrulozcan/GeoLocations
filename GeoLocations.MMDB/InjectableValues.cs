using System.Collections.Generic;

namespace GeoLocations.MMDB
{
	public sealed class InjectableValues
	{
		#region Properties

		internal IDictionary<string, object> Values { get; } = new Dictionary<string, object>();

		#endregion

		#region Methods

		public void AddValue(string key, object value)
		{
			this.Values.Add(key, value);
		}

		#endregion
	}
}