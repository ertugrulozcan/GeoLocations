using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoLocations.Infrastructure.Extensions
{
	public static class TaskExtensions
	{
		#region Methods

		public static Task ForEachAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> action)
		{
			return Task.WhenAll(source.Select(action));
		}
		
		public static Task ForEachAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> action)
		{
			return Task.WhenAll(source.Select(action));
		}

		#endregion
	}
}