using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Configuration;
using GeoLocations.Core.Extensions;
using GeoLocations.Core.Models;
using GeoLocations.Dao.Repositories;
using GeoLocations.Infrastructure.Extensions;
using GeoLocations.Dao.Extensions;
using GeoLocations.Dao.Models;
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
			return await this.CancellableFindAsync(ip);
		}
		
		public async ValueTask<GeoLocation> FindAsync(string ip, CancellationToken cancellationToken)
		{
			return await this.CancellableFindAsync(ip, cancellationToken);
		}
		
		private async ValueTask<GeoLocation> CancellableFindAsync(string ip, CancellationToken? cancellationToken = null)
		{
			var database = this.repositoryContextFactory.ResolveRepositoryContext();
			var query = database.GeoLocations
				.Include(x => x.City)
				.ThenInclude(x => x.Names)
				.Include(x => x.Continent)
				.ThenInclude(x => x.Names)
				.Include(x => x.Country)
				.ThenInclude(x => x.Names)
				.Include(x => x.County)
				.ThenInclude(x => x.Names)
				.Include(x => x.Region)
				.ThenInclude(x => x.Names)
				.AsSplitQuery()
				.AsNoTracking();

			if (cancellationToken == null)
			{
				var entity = await query.FirstOrDefaultAsync(x => x.IP == ip);
				return entity?.ToModel();
			}
			else
			{
				var entity = await query.FirstOrDefaultAsync(x => x.IP == ip, cancellationToken.Value);
				return entity?.ToModel();	
			}
		}

		public async ValueTask InsertAsync(IEnumerable<GeoLocation> items)
		{
			await this.CancellableInsertAsync(items);
		}
		
		public async ValueTask InsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken)
		{
			await this.CancellableInsertAsync(items, cancellationToken);
		}
		
		private async ValueTask CancellableInsertAsync(IEnumerable<GeoLocation> items, CancellationToken? cancellationToken = null)
		{
			var database = this.repositoryContextFactory.ResolveRepositoryContext();
			var entities = items.Select(x => x.ToEntity());
			if (cancellationToken == null)
			{
				await database.GeoLocations.AddRangeAsync(entities);
				await database.SaveChangesAsync();	
			}
			else
			{
				await database.GeoLocations.AddRangeAsync(entities, cancellationToken.Value);
				await database.SaveChangesAsync(cancellationToken.Value);	
			}
		}

		public async ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items)
		{
			await this.CancellableBulkInsertAsync(items);
		}
		
		public async ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken)
		{
			await this.CancellableBulkInsertAsync(items, cancellationToken);
		}
		
		private ValueTask CancellableBulkInsertAsync(IEnumerable<GeoLocation> items, CancellationToken? cancellationToken = null)
		{
			throw new NotImplementedException();
		}

		public async ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items)
		{
			await this.CancellableBatchInsertAsync(items);
		}
		
		public async ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken)
		{
			await this.CancellableBatchInsertAsync(items, cancellationToken);
		}
		
		private async ValueTask CancellableBatchInsertAsync(IEnumerable<GeoLocation> items, CancellationToken? cancellationToken = null)
		{
			if (this.geoLocationOptions.BatchSize <= 0)
			{
				throw new ArgumentException("BatchSize must be greater than zero");
			}
			
			try
			{
				this.logger.Info("Data extracting...");
				var entities = items.Select(x => x.ToEntity());
				var batches = entities.Batch(this.geoLocationOptions.BatchSize).ToArray();
				
				await using (var context = this.repositoryContextFactory.ResolveRepositoryContext())
				{
					var totalBatchCount = batches.Length;
					var batchNo = 1;
					var stopwatch = Stopwatch.StartNew();
					foreach (var batch in batches)
					{
						if (cancellationToken == null)
						{
							await BatchInsertAsync(context, batch);
						}
						else
						{
							await BatchInsertAsync(context, batch, cancellationToken.Value);	
						}

						var estimatedRemainingTime = TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds * (totalBatchCount - batchNo));
						var completedPercentage = (batchNo - 1) * 100.0d / totalBatchCount;
						this.logger.Info($"Batch {batchNo++} completed (%{completedPercentage:F2}) (Elapsed {stopwatch.Elapsed.ToHumanReadableString()}, Estimated remaining time {estimatedRemainingTime.ToHumanReadableString()})");
						stopwatch.Restart();
					}
				}
			}
			catch (Exception ex)
			{
				this.logger.LogException(ex);
				throw;
			}
		}

		private async ValueTask BatchInsertAsync(RepositoryContext context, GeoLocationEntity[] array)
		{
			await this.CancellableBatchInsertAsync(context, array);
		}
		
		private async ValueTask BatchInsertAsync(RepositoryContext context, GeoLocationEntity[] array, CancellationToken cancellationToken)
		{
			await this.CancellableBatchInsertAsync(context, array, cancellationToken);
		}
		
		private async ValueTask CancellableBatchInsertAsync(RepositoryContext context, GeoLocationEntity[] array, CancellationToken? cancellationToken = null)
		{
			if (cancellationToken == null)
			{
				await using (var transaction = await context.Database.BeginTransactionAsync())
				{
					try
					{
						this.logger.Info("Transaction started...");
						context.ChangeTracker.AutoDetectChangesEnabled = false;
						context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
						await context.GeoLocations.AddRangeAsync(array);
						await context.SaveChangesAsync();
						await transaction.CommitAsync();
						this.logger.Info("Transaction completed");
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						this.logger.Error("Transaction failed: ");
						this.logger.LogException(ex);
					}
				}
			}
			else
			{
				await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken.Value))
				{
					try
					{
						this.logger.Info("Transaction started...");
						context.ChangeTracker.AutoDetectChangesEnabled = false;
						context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
						await context.GeoLocations.AddRangeAsync(array);
						await context.SaveChangesAsync(cancellationToken.Value);
						await transaction.CommitAsync(cancellationToken.Value);
						this.logger.Info("Transaction completed");
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync(cancellationToken.Value);
						this.logger.Error("Transaction failed: ");
						this.logger.LogException(ex);
					}
				}	
			}
		}

		public async ValueTask RawBinaryCopyAsync(byte[] bytes)
		{
			await CancellableRawBinaryCopyAsync(bytes);
		}
		
		public async ValueTask RawBinaryCopyAsync(byte[] bytes, CancellationToken cancellationToken)
		{
			await CancellableRawBinaryCopyAsync(bytes, cancellationToken);
		}
		
		private async ValueTask CancellableRawBinaryCopyAsync(byte[] bytes, CancellationToken? cancellationToken = null)
		{
			await using (var context = this.repositoryContextFactory.ResolveRepositoryContext())
			{
				var entityTypes = context.Model.GetEntityTypes();
				var geoLocationEntityType = entityTypes.FirstOrDefault(x => x.ClrType == typeof(GeoLocationEntity));
				if (geoLocationEntityType != null)
				{
					var destinationTableName = geoLocationEntityType.GetTableName();
					var connectionString = this.geoLocationOptions.ConnectionString;

					if (cancellationToken == null)
					{
						await RawBinaryCopyAsync(bytes, connectionString, destinationTableName);
					}
					else
					{
						await RawBinaryCopyAsync(bytes, connectionString, destinationTableName, cancellationToken.Value);	
					}
				}
			}
		}
		
		private static async ValueTask RawBinaryCopyAsync(byte[] bytes, string connectionString, string destinationTableName)
		{
			await CancellableRawBinaryCopyAsync(bytes, connectionString, destinationTableName);
		}
		
		private static async ValueTask RawBinaryCopyAsync(byte[] bytes, string connectionString, string destinationTableName, CancellationToken cancellationToken)
		{
			await CancellableRawBinaryCopyAsync(bytes, connectionString, destinationTableName, cancellationToken);
		}
		
		private static async ValueTask CancellableRawBinaryCopyAsync(byte[] bytes, string connectionString, string destinationTableName, CancellationToken? cancellationToken = null)
		{
			try
			{
				await using (var connection = new NpgsqlConnection(connectionString))
				{
					if (cancellationToken == null)
					{
						await connection.OpenAsync();
					}
					else
					{
						await connection.OpenAsync(cancellationToken.Value);	
					}
					
					await using (var outStream = connection.BeginRawBinaryCopy($"COPY \"{destinationTableName}\" FROM STDIN (FORMAT BINARY)")) 
					{
						if (cancellationToken == null)
						{
							await outStream.WriteAsync(bytes, 0, bytes.Length);
						}
						else
						{
							await outStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken.Value);
						}
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
			return await this.CancellableClearAllAsync();
		}
		
		public async ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken)
		{
			return await this.CancellableClearAllAsync(cancellationToken);
		}
		
		private async ValueTask<bool> CancellableClearAllAsync(CancellationToken? cancellationToken = null)
		{
			try
			{
				var database = this.repositoryContextFactory.ResolveRepositoryContext();
				var tableNames = database.GetTableNames();
				foreach (var tableName in tableNames)
				{
					if (cancellationToken == null)
					{
						await database.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE public.\"{tableName}\" CASCADE");
					}
					else
					{
						await database.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE public.\"{tableName}\" CASCADE", cancellationToken.Value);	
					}
				}

				if (cancellationToken == null)
				{
					await database.SaveChangesAsync();
				}
				else
				{
					await database.SaveChangesAsync(cancellationToken.Value);	
				}
				
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