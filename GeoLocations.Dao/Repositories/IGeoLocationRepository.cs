using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Dao.Repositories
{
	public interface IGeoLocationRepository : 
		IGeoLocationReaderRepository, 
		IGeoLocationWriterRepository, 
		IGeoLocationBulkWriterRepository, 
		IGeoLocationBatchWriterRepository, 
		IGeoLocationRawCopyWriterRepository,
		IGeoLocationCleanerRepository
	{
		
	}

	public interface IGeoLocationReaderRepository
	{
		ValueTask<GeoLocation> FindAsync(string ip);
		
		ValueTask<GeoLocation> FindAsync(string ip, CancellationToken cancellationToken);
	}

	public interface IGeoLocationWriterRepository
	{
		ValueTask InsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask InsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);
	}
	
	public interface IGeoLocationBulkWriterRepository
	{
		ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BulkInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);
	}
	
	public interface IGeoLocationBatchWriterRepository
	{
		ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items);
		
		ValueTask BatchInsertAsync(IEnumerable<GeoLocation> items, CancellationToken cancellationToken);
	}
	
	public interface IGeoLocationRawCopyWriterRepository
	{
		ValueTask RawBinaryCopyAsync(byte[] bytes);
		
		ValueTask RawBinaryCopyAsync(byte[] bytes, CancellationToken cancellationToken);
	}
	
	public interface IGeoLocationCleanerRepository
	{
		ValueTask<bool> ClearAllAsync();
		
		ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken);
	}
}