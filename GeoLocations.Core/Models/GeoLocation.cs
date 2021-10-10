namespace GeoLocations.Core.Models
{
	public class GeoLocation : ResourceBase
	{
		#region Properties

		public string IP { get; set; }
		
		public IpVersion IpVersion { get; set; }
		
		public City City { get; set; }
		
		public Continent Continent { get; set; }
		
		public Country Country { get; set; }
		
		public Region Region { get; set; }

		public County County { get; set; }
		
		public Coordinate Location { get; set; }
		
		public string ZipCode { get; set; }
		
		public string TimeZone { get; set; }
		
		public string Currency { get; set; }
		
		public string WeatherStationCode { get; set; }
		
		public string ISP { get; set; }
		
		public string Organization { get; set; }

		#endregion
	}
}