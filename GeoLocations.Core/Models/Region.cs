using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public class Region : ILocalized
	{
		#region Properties

		public IEnumerable<LocalizedVariable<string>> Names { get; set; }

		#endregion
	}
}