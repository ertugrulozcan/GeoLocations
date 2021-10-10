using System.Collections.Generic;
using System.Linq;
using GeoLocations.PostgreSQL.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoLocations.PostgreSQL.Repositories
{
	public sealed class RepositoryContext : DbContext
	{
		#region DbSets

		public DbSet<GeoLocationEntity> GeoLocations { get; set; }

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options"></param>
		public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
		{
			
		}

		#endregion

		#region Methods

		internal IEnumerable<string> GetTableNames()
		{
			var entityTypes = this.Model.GetEntityTypes();
			return entityTypes.Select(x => x.GetTableName());
		}

		#endregion
	}
}