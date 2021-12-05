using System;
using GeoLocations.Abstractions.Configuration;
using GeoLocations.Abstractions.Services;
using GeoLocations.Dao.Repositories;
using GeoLocations.Extensions.AspNetCore.Configuration;
using GeoLocations.Extensions.AspNetCore.Jobs;
using GeoLocations.Infrastructure.Services;
using GeoLocations.PostgreSQL.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GeoLocations.Extensions.AspNetCore
{
	public static class Bootstrapper
	{
		#region Methods

		public static void AddGeoLocations(this IServiceCollection services, Action<GeoLocationOptions> optionsAction = null)
		{
			if (optionsAction != null)
			{
				var options = new GeoLocationOptions();
				optionsAction(options);
				services.Configure(optionsAction);
				services.ConfigureGeoLocations(options);
			}
		}

		public static void AddGeoLocations(this IServiceCollection services, GeoLocationOptions options)
		{
			services.ConfigureGeoLocations(options);
		}
		
		private static void ConfigureGeoLocations(this IServiceCollection services, GeoLocationOptions options)
		{
			services.AddSingleton<IGeoLocationOptions>(options);
			services.AddSingleton<IPostgreDatabaseSettings>(new PostgreDatabaseSettings { ConnectionString = options.ConnectionString });
			services.AddDbContext<PostgreSQL.Repositories.RepositoryContext>(context => 
					context.UseNpgsql(options.ConnectionString, b => b.MigrationsAssembly("GeoLocations.PostgreSQL")),
				contextLifetime: ServiceLifetime.Transient, 
				optionsLifetime: ServiceLifetime.Transient);
			services.AddSingleton<PostgreSQL.Repositories.IRepositoryContextFactory, PostgreSQL.Repositories.RepositoryContextFactory>();
			services.AddTransient<IGeoLocationRepository, PostgreSQL.Repositories.GeoLocationRepository>();
			services.AddTransient<IGeoLocationBatchWriterRepository, PostgreSQL.Repositories.GeoLocationRepository>();
			services.AddScoped<IGeoLocationService, GeoLocationService>();
			services.AddSingleton<IMasterDatabaseProvider, LocalStorageDatabaseProvider>();

			services.ConfigureScheduledJob(options);
		}

		private static void ConfigureScheduledJob(this IServiceCollection services, GeoLocationOptions options)
		{
			services.AddQuartz(quartz =>
			{
				quartz.SchedulerId = "GeoLocations-Scheduler";
				quartz.SchedulerName = "GeoLocations Quartz Scheduler";
				quartz.UseMicrosoftDependencyInjectionJobFactory();
				quartz.UseSimpleTypeLoader();
				quartz.UseInMemoryStore();
				quartz.UseDefaultThreadPool(tp =>
				{
					tp.MaxConcurrency = 10;
				});

				var jobKey = new JobKey("geolocation-job", "geolocation-job-group");
				quartz.AddJob<GeoLocationUpdaterJob>(jobKey, j => j
					.WithDescription("GeolocationJob")
				);

				quartz.AddTrigger(t => t
					.WithIdentity("GeolocationJobCronTrigger")    
					.ForJob(jobKey)
					.WithCronSchedule(options.Cron)
					.WithDescription("GeolocationJobTrigger")
				);
			});
			
			services.AddQuartzServer(quartz =>
			{
				// When shutting down we want jobs to complete gracefully
				quartz.WaitForJobsToComplete = true;
			});
		}

		#endregion
	}
}