using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using GeoLocations.Abstractions.Configuration;
using GeoLocations.Core.Extensions;
using GeoLocations.Core.Models;
using GeoLocations.Dao.Extensions;
using GeoLocations.Dao.Models;
using GeoLocations.Dao.Repositories;
using GeoLocations.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GeoLocations.Dapper.Repositories
{
	public class GeoLocationRepository : IGeoLocationBatchWriterRepository
	{
		#region Constants

		private const string CitiesTableName = "Cities";
		private const string CityNamesTableName = "CityNames";
		private const string CountriesTableName = "Countries";
		private const string CountryNamesTableName = "CountryNames";
		private const string ContinentsTableName = "Continents";
		private const string ContinentNamesTableName = "ContinentNames";
		private const string CountiesTableName = "Counties";
		private const string CountyNamesTableName = "CountyNames";
		private const string RegionsTableName = "Regions";
		private const string RegionNamesTableName = "RegionNames";
		private const string GeoLocationsTableName = "GeoLocations";

		#endregion
		
		#region Services

		private readonly IGeoLocationOptions geoLocationOptions;
		private readonly ILogger<GeoLocationRepository> logger;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="geoLocationOptions"></param>
		/// <param name="logger"></param>
		public GeoLocationRepository(
			IGeoLocationOptions geoLocationOptions,
			ILogger<GeoLocationRepository> logger)
		{
			this.geoLocationOptions = geoLocationOptions;
			this.logger = logger;
		}

		#endregion
		
		#region Public Methods
		
		public async ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items)
		{
			if (this.geoLocationOptions.BatchSize <= 0)
			{
				throw new ArgumentException("BatchSize must be greater than zero");
			}

			await using (var connection = new NpgsqlConnection(this.geoLocationOptions.ConnectionString))
			{
				await connection.OpenAsync();
				
				var indexes = await ListIndexDefinitionsAsync(connection);
				var foreignKeys = await ListForeignKeysAsync(connection);
				var tableNames = await this.GetTableNamesAsync(connection);
				
				try
				{
					await SetTablesUnloggedMode(connection, tableNames);
					this.logger.Info("Tables set as un-logged.");
					
					await DropIndexesAsync(connection, indexes);
					this.logger.Info("All indexes are temporarily dropped.");
					
					await DropForeignKeysAsync(connection, foreignKeys);
					this.logger.Info("Foreign key constraints are temporarily dropped.");
					
					await DisableAllTriggersAsync(connection, tableNames);
					this.logger.Info("All triggers are temporarily disabled.");
					
					this.logger.Info("Subdivisions writing...");
					var entities = items.Select(x => x.ToEntity()).ToArray();
					await this.InsertAllSubdivisionsAsync(entities);

					this.logger.Info("Batches preparing...");
					var batches = entities.Batch(this.geoLocationOptions.BatchSize).ToArray();
					var totalBatchCount = batches.Length;
					var batchNo = 1;

					var transaction = await connection.BeginTransactionAsync();
					this.logger.Info("Insert operations started.");
					var stopwatch = Stopwatch.StartNew();

					foreach (var batch in batches)
					{
						await this.BulkInsertAsync(batch, connection);

						var estimatedRemainingTime = TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds * (totalBatchCount - batchNo));
						this.logger.Info(
							$"Batch {batchNo++} completed (%{((batchNo - 1) * 100.0d / totalBatchCount):F2}) (Elapsed {stopwatch.Elapsed.ToHumanReadableString()}, Estimated remaining time {estimatedRemainingTime.ToHumanReadableString()})");
						stopwatch.Restart();
					}

					stopwatch.Stop();
					await transaction.CommitAsync();
				}
				catch (Exception ex)
				{
					this.logger.LogException(ex);
					throw;
				}
				finally
				{
					await RecreateIndexesAsync(connection, indexes);
					this.logger.Info("Indexes recreated.");
					
					await RecreateForeignKeysAsync(connection, foreignKeys);
					this.logger.Info("Foreign keys recreated.");
					
					await EnableAllTriggersAsync(connection, tableNames);
					this.logger.Info("Triggers enabled.");
					
					await SetTablesLoggedMode(connection, tableNames);
					this.logger.Info("Tables set as logged.");
				}
			}
		}
		
		public async ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken)
		{
			await this.BatchInsertAsync(items);
		}
		
		public async ValueTask<bool> ClearAllAsync()
		{
			try
			{
				await using (var connection = new NpgsqlConnection(this.geoLocationOptions.ConnectionString))
				{
					await connection.OpenAsync();
					var tableNames = await this.GetTableNamesAsync(connection);
					foreach (var tableName in tableNames)
					{
						var query = $"TRUNCATE TABLE \"{tableName}\" CASCADE;";
						await connection.ExecuteAsync(query);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				this.logger.LogException(ex);
				return false;
			}
		}

		public async ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken)
		{
			return await this.ClearAllAsync();
		}
		
		#endregion
		
		#region Private Methods

		private static async ValueTask SetTablesLoggedMode(IDbConnection connection, IEnumerable<string> tableNames)
		{
			foreach (var tableName in tableNames)
			{
				await SetTableLoggedMode(connection, tableName);
			}
		}
		
		private static async ValueTask SetTableLoggedMode(IDbConnection connection, string targetTableName)
		{
			await connection.ExecuteAsync($"ALTER TABLE \"{targetTableName}\" SET LOGGED");
		}
		
		private static async ValueTask SetTablesUnloggedMode(IDbConnection connection, IEnumerable<string> tableNames)
		{
			foreach (var tableName in tableNames)
			{
				await SetTableUnloggedMode(connection, tableName);
			}
		}
		
		private static async ValueTask SetTableUnloggedMode(IDbConnection connection, string targetTableName)
		{
			await connection.ExecuteAsync($"ALTER TABLE \"{targetTableName}\" SET UNLOGGED");
		}

		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		private static async ValueTask<dynamic[]> ListIndexDefinitionsAsync(IDbConnection connection)
		{
			var indexes = await connection.QueryAsync("SELECT tablename, indexname, indexdef FROM pg_indexes WHERE schemaname = 'public' ORDER BY tablename, indexname;");
			return indexes?.Where(x => !((string)x.indexname.ToString()).StartsWith("PK_")).ToArray();
		}
		
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private static async ValueTask DropIndexesAsync(IDbConnection connection, IEnumerable<dynamic> indexDefinitions)
		{
			if (indexDefinitions != null && indexDefinitions.Any())
			{
				var query = $"DROP INDEX {string.Join(", ", indexDefinitions.Select(x => $"\"{x.indexname}\""))}";
				await connection.ExecuteAsync(query);
			}
		}
		
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private static async ValueTask RecreateIndexesAsync(IDbConnection connection, IEnumerable<dynamic> indexDefinitions)
		{
			if (indexDefinitions != null && indexDefinitions.Any())
			{
				foreach (var indexDefinition in indexDefinitions)
				{
					var createIndexQuery = string.Empty;
					createIndexQuery += indexDefinition.indexdef.ToString();
					await connection.ExecuteAsync(createIndexQuery);	
				}
			}
		}
		
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		private static async ValueTask<dynamic[]> ListForeignKeysAsync(IDbConnection connection)
		{
			var columns = new[]
			{
				"tc.table_schema", 
				"tc.constraint_name", 
				"tc.table_name", 
				"kcu.column_name", 
				"ccu.table_schema AS foreign_table_schema",
				"ccu.table_name AS foreign_table_name",
				"ccu.column_name AS foreign_column_name"
			};

			var query =
				$"SELECT {string.Join(", ", columns)} " +
				"FROM information_schema.table_constraints AS tc " +
				"JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema " +
				"JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema " +
				"WHERE tc.constraint_type = 'FOREIGN KEY';";
			
			var foreignKeys = await connection.QueryAsync(query);
			return foreignKeys?.ToArray();
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private static async ValueTask DropForeignKeysAsync(IDbConnection connection, IEnumerable<dynamic> foreignKeys)
		{
			if (foreignKeys != null && foreignKeys.Any())
			{
				foreach (var foreignKeyDefinition in foreignKeys)
				{
					var query = $"ALTER TABLE \"{foreignKeyDefinition.table_name}\" DROP CONSTRAINT \"{foreignKeyDefinition.constraint_name}\"";
					await connection.ExecuteAsync(query);	
				}
			}
		}
		
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private static async ValueTask RecreateForeignKeysAsync(IDbConnection connection, IEnumerable<dynamic> foreignKeys)
		{
			if (foreignKeys != null && foreignKeys.Any())
			{
				foreach (var foreignKeyDefinition in foreignKeys)
				{
					var query = 
						$"ALTER TABLE \"{foreignKeyDefinition.table_name}\" " +
						$"ADD CONSTRAINT \"{foreignKeyDefinition.constraint_name}\" " +
						$"FOREIGN KEY (\"{foreignKeyDefinition.column_name}\") " +
						$"REFERENCES \"{foreignKeyDefinition.foreign_table_name}\"(\"{foreignKeyDefinition.foreign_column_name}\")";
					
					await connection.ExecuteAsync(query);
				}
			}
		}

		private static async ValueTask DisableAllTriggersAsync(IDbConnection connection, IEnumerable<string> tableNames)
		{
			foreach (var targetTableName in tableNames)
			{
				await connection.ExecuteAsync($"ALTER TABLE \"{targetTableName}\" DISABLE TRIGGER ALL");	
			}
		}
		
		private static async ValueTask EnableAllTriggersAsync(IDbConnection connection, IEnumerable<string> tableNames)
		{
			foreach (var targetTableName in tableNames)
			{
				await connection.ExecuteAsync($"ALTER TABLE \"{targetTableName}\" ENABLE TRIGGER ALL");	
			}
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask InsertCitiesAsync(IEnumerable<CityEntity> cities, IDbConnection connection)
		{
			var cityEntities = cities as CityEntity[] ?? cities.ToArray();
			if (cityEntities.Any())
			{
				var insertCitiesQuery = $"INSERT INTO \"{CitiesTableName}\" (\"Id\") VALUES (@Id);";
				await connection.ExecuteAsync(insertCitiesQuery, cityEntities.Select(x => new { x.Id }));
				var insertCityNamesQuery = $"INSERT INTO \"{CityNamesTableName}\" (\"CityId\", \"Name\", \"Locale\") VALUES (@CityId, @Name, @Locale);";
				await connection.ExecuteAsync(insertCityNamesQuery, cityEntities.SelectMany(x => x.Names).Select(x => new { CityId = x.City.Id, x.Name, x.Locale }));	
			}
		}
		
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask InsertContinentsAsync(IEnumerable<ContinentEntity> continents, IDbConnection connection)
		{
			var continentEntities = continents as ContinentEntity[] ?? continents.ToArray();
			if (continentEntities.Any())
			{
				var insertContinentsQuery = $"INSERT INTO \"{ContinentsTableName}\" (\"Id\", \"Code\", \"GeoNameId\") VALUES (@Id, @Code, @GeoNameId);";
				await connection.ExecuteAsync(insertContinentsQuery, continentEntities.Select(x => new { x.Id, x.Code, x.GeoNameId }));
				var insertContinentNamesQuery = $"INSERT INTO \"{ContinentNamesTableName}\" (\"ContinentId\", \"Name\", \"Locale\") VALUES (@ContinentId, @Name, @Locale);";
				await connection.ExecuteAsync(insertContinentNamesQuery, continentEntities.SelectMany(x => x.Names).Select(x => new { ContinentId = x.Continent.Id, x.Name, x.Locale }));	
			}
		}
		
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask InsertCountriesAsync(IEnumerable<CountryEntity> countries, IDbConnection connection)
		{
			var countryEntities = countries as CountryEntity[] ?? countries.ToArray();
			if (countryEntities.Any())
			{
				var insertCountriesQuery = $"INSERT INTO \"{CountriesTableName}\" (\"Id\", \"ISOCode\", \"GeoNameId\", \"IsInEuropeanUnion\", \"PhoneCode\") VALUES (@Id, @ISOCode, @GeoNameId, @IsInEuropeanUnion, @PhoneCode);";
				await connection.ExecuteAsync(insertCountriesQuery, countryEntities.Select(x => new { x.Id, x.ISOCode, x.GeoNameId, x.IsInEuropeanUnion, x.PhoneCode }));
				var insertCountryNamesQuery = $"INSERT INTO \"{CountryNamesTableName}\" (\"CountryId\", \"Name\", \"Locale\") VALUES (@CountryId, @Name, @Locale);";
				await connection.ExecuteAsync(insertCountryNamesQuery, countryEntities.SelectMany(x => x.Names).Select(x => new { CountryId = x.Country.Id, x.Name, x.Locale }));	
			}
		}
		
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask InsertCountiesAsync(IEnumerable<CountyEntity> counties, IDbConnection connection)
		{
			var countyEntities = counties as CountyEntity[] ?? counties.ToArray();
			if (countyEntities.Any())
			{
				var insertCountiesQuery = $"INSERT INTO \"{CountiesTableName}\" (\"Id\") VALUES (@Id);";
				await connection.ExecuteAsync(insertCountiesQuery, countyEntities.Select(x => new { x.Id }));
				var insertCountyNamesQuery = $"INSERT INTO \"{CountyNamesTableName}\" (\"CountyId\", \"Name\", \"Locale\") VALUES (@CountyId, @Name, @Locale);";
				await connection.ExecuteAsync(insertCountyNamesQuery, countyEntities.SelectMany(x => x.Names).Select(x => new { CountyId = x.County.Id, x.Name, x.Locale }));	
			}
		}
		
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask InsertRegionsAsync(IEnumerable<RegionEntity> regions, IDbConnection connection)
		{
			var regionEntities = regions as RegionEntity[] ?? regions.ToArray();
			if (regionEntities.Any())
			{
				var insertRegionsQuery = $"INSERT INTO \"{RegionsTableName}\" (\"Id\") VALUES (@Id);";
				await connection.ExecuteAsync(insertRegionsQuery, regionEntities.Select(x => new { x.Id }));
				var insertRegionNamesQuery = $"INSERT INTO \"{RegionNamesTableName}\" (\"RegionId\", \"Name\", \"Locale\") VALUES (@RegionId, @Name, @Locale);";
				await connection.ExecuteAsync(insertRegionNamesQuery, regionEntities.SelectMany(x => x.Names).Select(x => new { RegionId = x.Region.Id, x.Name, x.Locale }));	
			}
		}

		private async ValueTask InsertAllSubdivisionsAsync(IEnumerable<GeoLocationEntity> items)
		{
			await using (var connection = new NpgsqlConnection(this.geoLocationOptions.ConnectionString))
			{
				await connection.OpenAsync();
				await this.InsertAllSubdivisionsAsync(items, connection);
			}
		}
		
		private async ValueTask InsertAllSubdivisionsAsync(IEnumerable<GeoLocationEntity> items, IDbConnection connection)
		{
			var entities = items as GeoLocationEntity[] ?? items.ToArray();
			
			// Cities
			var cities = entities.Where(x => x.City != null).Select(x => x.City).DistinctBy(x => x.Id);
			await this.InsertCitiesAsync(cities, connection);
				
			// Continents
			var continents = entities.Where(x => x.Continent != null).Select(x => x.Continent).DistinctBy(x => x.Id);
			await this.InsertContinentsAsync(continents, connection);
				
			// Countries
			var countries = entities.Where(x => x.Country != null).Select(x => x.Country).DistinctBy(x => x.Id);
			await this.InsertCountriesAsync(countries, connection);
				
			// Counties
			var counties = entities.Where(x => x.County != null).Select(x => x.County).DistinctBy(x => x.Id);
			await this.InsertCountiesAsync(counties, connection);
				
			// Regions
			var regions = entities.Where(x => x.Region != null).Select(x => x.Region).DistinctBy(x => x.Id);
			await this.InsertRegionsAsync(regions, connection);
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		private async ValueTask BulkInsertAsync(IEnumerable<GeoLocationEntity> entities, IDbConnection connection)
		{
			try
			{
				var insertGeoLocationsQuery = 
					$"INSERT INTO \"{GeoLocationsTableName}\" (\"IP\", \"IpVersion\", \"Latitude\", \"Longitude\", \"CityId\", \"ContinentId\", \"CountryId\", \"RegionId\", \"CountyId\", \"ZipCode\", \"TimeZone\", \"Currency\", \"WeatherStationCode\", \"ISP\", \"Organization\") " +
					"VALUES (@IP, @IpVersion, @Latitude, @Longitude, @CityId, @ContinentId, @CountryId, @RegionId, @CountyId, @ZipCode, @TimeZone, @Currency, @WeatherStationCode, @ISP, @Organization);";

				await connection.ExecuteAsync(
					insertGeoLocationsQuery, 
					entities.Select(x => new
					{
						x.IP,
						x.IpVersion, 
						x.Latitude, 
						x.Longitude, 
						CityId = x.City?.Id, 
						ContinentId = x.Continent?.Id, 
						CountryId = x.Country?.Id, 
						RegionId = x.Region?.Id, 
						CountyId = x.County?.Id, 
						x.ZipCode, 
						x.TimeZone, 
						x.Currency, 
						x.WeatherStationCode, 
						x.ISP, 
						x.Organization
					}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		internal async ValueTask<string[]> GetTableNamesAsync(IDbConnection connection)
		{
			var tableNames = await connection.QueryAsync("SELECT table_name FROM information_schema.tables WHERE table_schema='public';");
			return tableNames.Select(x => (string)x.table_name.ToString()).ToArray();
		}
		
		#endregion
	}
}