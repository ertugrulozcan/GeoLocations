using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public class Continent : ILocalized
	{
		#region Properties
		
		public string Code { get; set; }
		
		public long GeoNameId { get; set; }

		public IEnumerable<LocalizedVariable<string>> Names { get; set; }

		#endregion
	}
}