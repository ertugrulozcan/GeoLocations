using System.ComponentModel.DataAnnotations.Schema;

namespace GeoLocations.PostgreSQL.Models
{
	[Table("Continents")]
	public class ContinentEntity : LocalizedEntityBase<ContinentNamesEntity>
	{
		#region Properties

		public string Code { get; set; }
		
		public long GeoNameId { get; set; }

		#endregion
	}
	
	[Table("ContinentNames")]
	public class ContinentNamesEntity : LocaleEntity
	{
		#region Properties

		public int ContinentId { get; set; }
		
		public ContinentEntity Continent { get; set; }

		#endregion
	}
}