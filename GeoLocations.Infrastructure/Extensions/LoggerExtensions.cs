using System;
using Microsoft.Extensions.Logging;

namespace GeoLocations.Infrastructure.Extensions
{
	public static class LoggerExtensions
	{
		#region Methods

		public static void LogException<T>(this ILogger<T> logger, Exception ex)
		{
			if (ex != null)
			{
				logger.Log(LogLevel.Error, ex.StackTrace, ex.Message);
				logger.LogException(ex.InnerException);
			}
		}

		#endregion
	}
}