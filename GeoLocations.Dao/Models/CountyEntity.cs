using System.ComponentModel.DataAnnotations.Schema;

namespace GeoLocations.Dao.Models
{
	[Table("Counties")]
	public class CountyEntity : LocalizedEntityBase<CountyNamesEntity>
	{
		
	}
	
	[Table("CountyNames")]
	public class CountyNamesEntity : LocaleEntity
	{
		#region Properties

		public int CountyId { get; set; }
		
		public CountyEntity County { get; set; }

		#endregion
	}
}