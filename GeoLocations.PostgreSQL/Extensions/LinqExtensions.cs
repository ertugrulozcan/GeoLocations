using System.Collections.Generic;
using System.Linq;

namespace GeoLocations.PostgreSQL.Extensions
{
	public static class LinqExtensions
	{
		#region Methods

		public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> collection, int batchSize)
		{
			var array = collection as T[] ?? collection.ToArray();
			
			var totalCount = array.Count();
			if (totalCount <= batchSize)
			{
				yield return array;
			}

			var leftCount = totalCount % batchSize;
			var groupCount = (totalCount - leftCount) / batchSize;
			for (int i = 0; i < groupCount; i++)
			{
				yield return array.Skip(i * batchSize).Take(batchSize).ToArray();
			}

			if (leftCount > 0)
			{
				yield return array.TakeLast(leftCount).ToArray();	
			}
		}

		#endregion
	}
}