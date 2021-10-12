using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Core.Models;

namespace GeoLocations.Abstractions.Services
{
	public interface IMasterDatabaseProvider
	{
		byte[] GetBinaryDatabase();
		
		IEnumerable<GeoLocation> GetData(int? skip = null!, int? limit = null!);

		Task<IEnumerable<GeoLocation>> GetDataAsync(int? skip = null!, int? limit = null!, CancellationToken? cancellationToken = null);
	}
}