namespace GeoLocations.Dao.Models
{
	public abstract class LocaleEntity : EntityBase
	{
		#region Properties

		public string Name { get; set; }
		
		public string Locale { get; set; }

		#endregion
	}
}