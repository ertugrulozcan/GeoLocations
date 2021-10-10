using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace GeoLocations.WebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		/*
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		*/

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.ConfigureAppConfiguration((builderContext, config) =>
					{
						var env = builderContext.HostingEnvironment;

						config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
							.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
							.AddCommandLine(args);
					});
					
					webBuilder.UseStartup<Startup>();
				});
	}
}