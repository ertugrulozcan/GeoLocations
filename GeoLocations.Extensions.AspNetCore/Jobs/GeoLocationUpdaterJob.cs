using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Services;
using GeoLocations.Core.Extensions;
using GeoLocations.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GeoLocations.Extensions.AspNetCore.Jobs
{
	public class GeoLocationUpdaterJob : IJob
	{
		#region Services
		
		private readonly IGeoLocationService geoLocationService;
		private readonly IMasterDatabaseProvider masterDatabaseProvider;
		private readonly ILogger<GeoLocationUpdaterJob> logger;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="geoLocationService"></param>
		/// <param name="masterDatabaseProvider"></param>
		/// <param name="logger"></param>
		public GeoLocationUpdaterJob(
			IGeoLocationService geoLocationService, 
			IMasterDatabaseProvider masterDatabaseProvider,
			ILogger<GeoLocationUpdaterJob> logger)
		{
			this.geoLocationService = geoLocationService;
			this.masterDatabaseProvider = masterDatabaseProvider;
			this.logger = logger;
		}

		#endregion
		
		#region Methods
		
		public async Task Execute(IJobExecutionContext context)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				this.logger.Info("GeoLocationUpdaterJob started.");
				this.logger.Info("GeoLocation data fetching from master data provider...");

				var data = await this.masterDatabaseProvider.GetDataAsync(cancellationToken: context.CancellationToken);
				if (data != null)
				{
					this.logger.Info("GeoLocation data fetched.");
					this.logger.Info("Clearing operation started...");

					var isCleared = await this.geoLocationService.ClearAllAsync(context.CancellationToken);
					if (isCleared)
					{
						this.logger.Info("Clearing operation completed.");
						this.logger.Info("Loading operation started...");

						await this.geoLocationService.LoadAsync(data, context.CancellationToken);
					}
					else
					{
						this.logger.Error("Clearing operation failed!");
					}
				}
				else
				{
					this.logger.Error("Data could not fetched from master data provider!");
				}
			}
			catch (Exception ex)
			{
				this.logger.LogException(ex);
			}
			finally
			{
				stopwatch.Stop();
				
				this.logger.Info("GeoLocationUpdaterJob finished.");
				this.logger.Info($"Elapsed time: {stopwatch.Elapsed.ToHumanReadableString()}");
			}
		}

		#endregion
	}
}