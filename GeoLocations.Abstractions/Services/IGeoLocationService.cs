using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Abstractions.Services
{
	public interface IGeoLocationService
	{
		ValueTask<GeoLocation> FindAsync(string ip);
		
		ValueTask<GeoLocation> FindAsync(string ip, CancellationToken cancellationToken);
		
		ValueTask<bool> ClearAllAsync();
		
		ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken);
		
		ValueTask LoadAsync(IEnumerable<GeoLocation> data);
		
		ValueTask LoadAsync(IEnumerable<GeoLocation> data, CancellationToken cancellationToken);
		
		ValueTask LoadBinaryAsync(byte[] bytes);
		
		ValueTask LoadBinaryAsync(byte[] bytes, CancellationToken cancellationToken);
	}
}