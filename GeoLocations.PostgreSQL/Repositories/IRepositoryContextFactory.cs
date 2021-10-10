namespace GeoLocations.PostgreSQL.Repositories
{
	public interface IRepositoryContextFactory
	{
		RepositoryContext ResolveRepositoryContext();
	}
}