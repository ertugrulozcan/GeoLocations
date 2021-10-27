using GeoLocations.Abstractions.Configuration;

namespace GeoLocations.Extensions.AspNetCore.Configuration
{
	public class GeoLocationOptions : IGeoLocationOptions
	{
		#region Properties

		public string Cron { get; set; }

		public string ConnectionString { get; set; }
		
		public int BatchSize { get; set; }

		#endregion
	}
}