using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Dao.Repositories
{
	public interface IGeoLocationRepository
	{
		ValueTask<GeoLocation> FindAsync(string ip);
		
		ValueTask<GeoLocation> FindAsync(string ip, CancellationToken cancellationToken);

		ValueTask InsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask InsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);
		
		ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);
		
		ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);

		ValueTask RawBinaryCopyAsync(byte[] bytes);
		
		ValueTask RawBinaryCopyAsync(byte[] bytes, CancellationToken cancellationToken);
		
		ValueTask<bool> ClearAllAsync();
		
		ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken);
	}
}