using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Configuration;
using GeoLocations.Core.Extensions;
using GeoLocations.Core.Models;
using GeoLocations.Dao.Repositories;
using GeoLocations.PostgreSQL.Extensions;
using GeoLocations.PostgreSQL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GeoLocations.PostgreSQL.Repositories
{
	public class GeoLocationRepository : IGeoLocationRepository
	{
		#region Context

		private readonly IRepositoryContextFactory repositoryContextFactory;
		private readonly IGeoLocationOptions geoLocationOptions;
		private readonly ILogger<GeoLocationRepository> logger;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="repositoryContextFactory"></param>
		/// <param name="geoLocationOptions"></param>
		/// <param name="logger"></param>
		public GeoLocationRepository(
			IRepositoryContextFactory repositoryContextFactory, 
			IGeoLocationOptions geoLocationOptions,
			ILogger<GeoLocationRepository> logger)
		{
			this.repositoryContextFactory = repositoryContextFactory;
			this.geoLocationOptions = geoLocationOptions;
			this.logger = logger;
		}

		#endregion

		#region Methods
		
		public async ValueTask<GeoLocation> FindAsync(string ip)
		{
			var database = this.repositoryContextFactory.ResolveRepositoryContext();
			var entity = await database.GeoLocations.FirstOrDefaultAsync(x => x.IP == ip);
			return entity?.ToModel();
		}

		public async ValueTask InsertAsync(IEnumerable<GeoLocation> items)
		{
			var database = this.repositoryContextFactory.ResolveRepositoryContext();
			var entities = items.Select(x => x.ToEntity());
			await database.GeoLocations.AddRangeAsync(entities);
			await database.SaveChangesAsync();
		}

		public ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items)
		{
			throw new NotImplementedException();
		}
		
		public async ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items)
		{
			try
			{
				const int BATCH_SIZE = 10000;
				this.logger.Log(LogLevel.Information, "Data extracting...");
				var entities = items.Select(x => x.ToEntity());
				var batches = entities.Batch(BATCH_SIZE).ToArray();
				
				await using (var context = this.repositoryContextFactory.ResolveRepositoryContext())
				{
					var batchNo = 1;
					var stopwatch = Stopwatch.StartNew();
					foreach (var batch in batches)
					{
						await BatchInsertAsync(context, batch);
						this.logger.Log(LogLevel.Information, $"Batch {batchNo++} completed (Elapsed {stopwatch.Elapsed.ToHumanReadableString()})");
						this.logger.Log(LogLevel.Information, $"Completed %{((batchNo - 1) * 100.0d / batches.Length):F2}");
						stopwatch.Restart();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public async ValueTask BatchInsertAsync(RepositoryContext context, GeoLocationEntity[] array)
		{
			await using (var transaction = await context.Database.BeginTransactionAsync())
			{
				try
				{
					this.logger.Log(LogLevel.Information, "Transaction started...");
					context.ChangeTracker.AutoDetectChangesEnabled = false;
					context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
					await context.GeoLocations.AddRangeAsync(array);
					await context.SaveChangesAsync();
					await transaction.CommitAsync();
					this.logger.Log(LogLevel.Information, "Transaction completed");
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					this.logger.Log(LogLevel.Error, "Transaction failed: {ex}", ex);
				}
			}
		}

		public async ValueTask RawBinaryCopyAsync(byte[] bytes)
		{
			await using (var context = this.repositoryContextFactory.ResolveRepositoryContext())
			{
				var entityTypes = context.Model.GetEntityTypes();
				var geoLocationEntityType = entityTypes.FirstOrDefault(x => x.ClrType == typeof(GeoLocationEntity));
				if (geoLocationEntityType != null)
				{
					var destinationTableName = geoLocationEntityType.GetTableName();
					var connectionString = this.geoLocationOptions.ConnectionString;
					await RawBinaryCopyAsync(bytes, connectionString, destinationTableName);
				}
			}
		}
		
		private static async ValueTask RawBinaryCopyAsync(byte[] bytes, string connectionString, string destinationTableName)
		{
			try
			{
				await using (var connection = new NpgsqlConnection(connectionString))
				{
					await connection.OpenAsync();
					await using (var outStream = connection.BeginRawBinaryCopy($"COPY \"{destinationTableName}\" FROM STDIN (FORMAT BINARY)")) 
					{
						outStream.Write(bytes, 0, bytes.Length);
					}	
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		public async ValueTask<bool> ClearAllAsync()
		{
			try
			{
				var database = this.repositoryContextFactory.ResolveRepositoryContext();
				var tableNames = database.GetTableNames();
				foreach (var tableName in tableNames)
				{
					await database.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE public.\"{tableName}\" CASCADE");
				}

				await database.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}

		#endregion
	}
}