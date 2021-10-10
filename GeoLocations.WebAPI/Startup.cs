using GeoLocations.Extensions.AspNetCore;
using GeoLocations.Extensions.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace GeoLocations.WebAPI
{
	public class Startup
	{
		#region Properties

		public IConfiguration Configuration { get; }

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"></param>
		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		#endregion
		
		#region Methods

		public void ConfigureServices(IServiceCollection services)
		{
			// Option configurations
			var geoLocationOptionsSection = Configuration.GetSection(nameof(GeoLocationOptions));
			var geoLocationOptions = geoLocationOptionsSection.Get<GeoLocationOptions>();
			services.AddOptions<GeoLocationOptions>().Bind(geoLocationOptionsSection);
			
			// Service registrations
			services.AddGeoLocations(geoLocationOptions); // OR => services.AddGeoLocations(options => { options.Cron = "0/3 * * * * ?"; });
			
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "GeoLocations API", Version = "v1" }); });
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeoLocation.WebAPI v1"));
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}

		#endregion
	}
}