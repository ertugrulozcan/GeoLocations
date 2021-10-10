using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public class Country : ILocalized
	{
		#region Properties
		
		public string ISOCode { get; set; }
		
		public long GeoNameId { get; set; }
		
		public bool IsInEuropeanUnion { get; set; }

		public IEnumerable<LocalizedVariable<string>> Names { get; set; }

		public string PhoneCode { get; set; }

		#endregion
	}
}