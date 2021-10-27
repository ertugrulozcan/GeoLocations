using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoLocations.Dao.Extensions
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
				yield break;
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
		
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var seenKeys = new HashSet<TKey>();
			foreach (var element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		#endregion
	}
}