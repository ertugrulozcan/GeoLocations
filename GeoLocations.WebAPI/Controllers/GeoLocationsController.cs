using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Services;
using GeoLocations.Core.Models;
using GeoLocations.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GeoLocations.WebAPI.Controllers
{
	[ApiController]
	[Route("api/v1/geolocations")]
	public class GeoLocationsController : ControllerBase
	{
		#region Services

		private readonly IGeoLocationService geoLocationService;
		private readonly ILogger<GeoLocationsController> logger;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="geoLocationService"></param>
		/// <param name="logger"></param>
		public GeoLocationsController(IGeoLocationService geoLocationService, ILogger<GeoLocationsController> logger)
		{
			this.geoLocationService = geoLocationService;
			this.logger = logger;
		}

		#endregion
		
		#region Methods

		[HttpGet]
		public async Task<ActionResult<IEnumerable<GeoLocation>>> Get([FromQuery] string ip)
		{
			try
			{
				if (string.IsNullOrEmpty(ip))
				{
					return this.BadRequest("IP address must be posted in query");
				}
				
				var geoLocation = await this.geoLocationService.FindAsync(ip);
				if (geoLocation != null)
				{
					return this.Ok(geoLocation);
				}
				else
				{
					return this.NotFound();
				}
			}
			catch (Exception ex)
			{
				this.logger.LogException(ex);
				return this.StatusCode(500, ex);
			}
		}

		#endregion
	}
}