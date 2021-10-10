namespace GeoLocations.Abstractions.Configuration
{
	public interface IGeoLocationOptions
	{
		#region Properties

		string Cron { get; set; }

		string ConnectionString { get; set; }

		#endregion
	}
}