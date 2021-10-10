using System;

namespace GeoLocations.MMDB.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class ParameterAttribute : Attribute
	{
		#region Properties

		public string ParameterName { get; }

		/// <summary>
		/// Whether to create the object even if the key is not present in the database. If this is false, the default value will be used (null for nullable types).
		/// </summary>
		public bool AlwaysCreate { get; }
		
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="alwaysCreate"></param>
		public ParameterAttribute(string parameterName, bool alwaysCreate = false)
		{
			this.ParameterName = parameterName;
			this.AlwaysCreate = alwaysCreate;
		}

		#endregion
	}
}