using System.Collections.Generic;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Abstractions.Services
{
	public interface IGeoLocationService
	{
		ValueTask<GeoLocation> FindAsync(string ip);
		
		ValueTask<bool> ClearAllAsync();
		
		ValueTask LoadAsync(IEnumerable<GeoLocation> data);
		
		ValueTask LoadBinaryAsync(byte[] bytes);
	}
}