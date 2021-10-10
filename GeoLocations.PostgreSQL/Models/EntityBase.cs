using System.ComponentModel.DataAnnotations;

namespace GeoLocations.PostgreSQL.Models
{
	public abstract class EntityBase : IEntity<int>
	{
		#region Properties

		[Key]
		public int Id { get; set; }

		#endregion
	}
}