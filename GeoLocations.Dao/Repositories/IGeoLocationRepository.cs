using System.Collections.Generic;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Dao.Repositories
{
	public interface IGeoLocationRepository
	{
		ValueTask<GeoLocation> FindAsync(string ip);

		ValueTask InsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items);

		ValueTask RawBinaryCopyAsync(byte[] bytes);
		
		ValueTask<bool> ClearAllAsync();
	}
}