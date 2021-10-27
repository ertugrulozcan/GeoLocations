using System.Collections.Generic;

namespace GeoLocations.Dao.Models
{
	public abstract class LocalizedEntityBase<TEntity> : EntityBase where TEntity : LocaleEntity
	{
		#region Properties

		public ICollection<TEntity> Names { get; set; }

		#endregion
	}
}