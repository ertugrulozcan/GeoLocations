using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public class City : ILocalized
	{
		#region Properties

		public IEnumerable<LocalizedVariable<string>> Names { get; set; }

		#endregion
	}
}