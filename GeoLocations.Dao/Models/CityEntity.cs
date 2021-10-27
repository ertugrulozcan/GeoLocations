using System.ComponentModel.DataAnnotations.Schema;

namespace GeoLocations.Dao.Models
{
	[Table("Cities")]
	public class CityEntity : LocalizedEntityBase<CityNamesEntity>
	{
		
	}
	
	[Table("CityNames")]
	public class CityNamesEntity : LocaleEntity
	{
		#region Properties

		public int CityId { get; set; }
		
		public CityEntity City { get; set; }

		#endregion
	}
}