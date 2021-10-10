using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public class County : ILocalized
	{
		#region Properties

		public IEnumerable<LocalizedVariable<string>> Names { get; set; }

		#endregion
	}
}