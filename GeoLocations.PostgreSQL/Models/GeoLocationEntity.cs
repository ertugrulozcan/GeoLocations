using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GeoLocations.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoLocations.PostgreSQL.Models
{
	[Index(nameof(IP))]
	public class GeoLocationEntity : EntityBase
	{
		#region Properties

		[Required]
		public string IP { get; set; }

		public IpVersion IpVersion { get; set; }
		
		public double? Latitude { get; set; }
		
		public double? Longitude { get; set; }
		
		public int? CityId { get; set; }
		
		[ForeignKey("CityId")]
		public virtual CityEntity City { get; set; }
		
		public int? ContinentId { get; set; }
		
		[ForeignKey("ContinentId")]
		public virtual ContinentEntity Continent { get; set; }
		
		public int? CountryId { get; set; }
		
		[ForeignKey("CountryId")]
		public virtual CountryEntity Country { get; set; }
		
		public int? RegionId { get; set; }
		
		[ForeignKey("RegionId")]
		public virtual RegionEntity Region { get; set; }
		
		public int? CountyId { get; set; }
		
		[ForeignKey("CountyId")]
		public virtual CountyEntity County { get; set; }

		public string ZipCode { get; set; }
		
		public string TimeZone { get; set; }
		
		public string Currency { get; set; }
		
		public string WeatherStationCode { get; set; }
		
		public string ISP { get; set; }
		
		public string Organization { get; set; }

		#endregion
	}
}