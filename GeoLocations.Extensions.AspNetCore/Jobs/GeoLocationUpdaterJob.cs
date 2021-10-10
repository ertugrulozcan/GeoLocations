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
				this.logger.Log(LogLevel.Information, "GeoLocationUpdaterJob started.");
				this.logger.Log(LogLevel.Information, "GeoLocation data fetching from master data provider...");

				var data = await this.masterDatabaseProvider.GetDataAsync();
				if (data != null)
				{
					this.logger.Log(LogLevel.Information, "GeoLocation data fetched.");
					this.logger.Log(LogLevel.Information, "Clearing operation started...");

					var isCleared = await this.geoLocationService.ClearAllAsync();
					if (isCleared)
					{
						this.logger.Log(LogLevel.Information, "Clearing operation completed.");
						this.logger.Log(LogLevel.Information, "Loading operation started...");

						await this.geoLocationService.LoadAsync(data);
					}
					else
					{
						this.logger.Log(LogLevel.Error, "Clearing operation failed!");
					}
				}
				else
				{
					this.logger.Log(LogLevel.Error, "Data could not fetched from master data provider!");
				}
			}
			catch (Exception ex)
			{
				this.logger.LogException(ex);
			}
			finally
			{
				stopwatch.Stop();
				
				this.logger.Log(LogLevel.Information, "GeoLocationUpdaterJob finished.");
				this.logger.Log(LogLevel.Information, $"Elapsed time: {stopwatch.Elapsed.ToHumanReadableString()}");
			}
		}

		#endregion
	}
}