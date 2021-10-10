namespace GeoLocations.PostgreSQL.Configuration
{
	public interface IPostgreDatabaseSettings
	{
		string ConnectionString { get; set; }
	}
	
	public class PostgreDatabaseSettings : IPostgreDatabaseSettings
	{
		public string ConnectionString { get; set; }
	}
}