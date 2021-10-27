using System.ComponentModel.DataAnnotations.Schema;

namespace GeoLocations.Dao.Models
{
	[Table("Regions")]
	public class RegionEntity : LocalizedEntityBase<RegionNamesEntity>
	{
		
	}
	
	[Table("RegionNames")]
	public class RegionNamesEntity : LocaleEntity
	{
		#region Properties

		public int RegionId { get; set; }
		
		public RegionEntity Region { get; set; }

		#endregion
	}
}