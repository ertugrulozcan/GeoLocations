using System;

namespace GeoLocations.MMDB.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class InjectAttribute : Attribute
	{
		#region Properties

		public string ParameterName { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameterName"></param>
		public InjectAttribute(string parameterName)
		{
			this.ParameterName = parameterName;
		}

		#endregion
	}
}