using System.Collections.Generic;

namespace GeoLocations.Core.Models
{
	public interface ILocalized
	{
		#region Properties

		IEnumerable<LocalizedVariable<string>> Names { get; set; }

		#endregion
	}
}