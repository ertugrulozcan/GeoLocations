using System.ComponentModel.DataAnnotations.Schema;

namespace GeoLocations.Dao.Models
{
	[Table("Countries")]
	public class CountryEntity : LocalizedEntityBase<CountryNamesEntity>
	{
		#region Properties

		public string ISOCode { get; set; }
		
		public long GeoNameId { get; set; }
		
		public bool IsInEuropeanUnion { get; set; }

		public string PhoneCode { get; set; }
		
		#endregion
	}
	
	[Table("CountryNames")]
	public class CountryNamesEntity : LocaleEntity
	{
		#region Properties

		public int CountryId { get; set; }
		
		public CountryEntity Country { get; set; }

		#endregion
	}
}