using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeoLocations.PostgreSQL.Repositories
{
	public class RepositoryContextDesignTimeFactory : IDesignTimeDbContextFactory<RepositoryContext>
	{
		public RepositoryContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<RepositoryContext>();
			optionsBuilder.UseNpgsql("Host=localhost;Database=geolocations;Username=postgres;Password=.Abcd1234!", b => b.MigrationsAssembly("GeoLocations.PostgreSQL"));

			return new RepositoryContext(optionsBuilder.Options);
		}
	}
}