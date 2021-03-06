using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Services;
using GeoLocations.Core.Models;
using GeoLocations.Dao.Repositories;

namespace GeoLocations.Infrastructure.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		#region Services

		private readonly IGeoLocationRepository repository;
		private readonly IGeoLocationBatchWriterRepository writerRepository;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="writerRepository"></param>
		public GeoLocationService(IGeoLocationRepository repository, IGeoLocationBatchWriterRepository writerRepository)
		{
			this.repository = repository;
			this.writerRepository = writerRepository;
		}

		#endregion
		
		#region Methods

		public async ValueTask<GeoLocation> FindAsync(string ip)
		{
			return await this.repository.FindAsync(ip);
		}
		
		public async ValueTask<GeoLocation> FindAsync(string ip, CancellationToken cancellationToken)
		{
			return await this.repository.FindAsync(ip, cancellationToken);
		}

		public async ValueTask LoadAsync(IEnumerable<GeoLocation> data)
		{
			await this.writerRepository.BatchInsertAsync(data);
		}
		
		public async ValueTask LoadAsync(IEnumerable<GeoLocation> data, CancellationToken cancellationToken)
		{
			await this.writerRepository.BatchInsertAsync(data, cancellationToken);
		}

		public async ValueTask LoadBinaryAsync(byte[] bytes)
		{
			await this.repository.RawBinaryCopyAsync(bytes);
		}

		public async ValueTask LoadBinaryAsync(byte[] bytes, CancellationToken cancellationToken)
		{
			await this.repository.RawBinaryCopyAsync(bytes, cancellationToken);
		}

		public async ValueTask<bool> ClearAllAsync()
		{
			return await this.repository.ClearAllAsync();
		}
		
		public async ValueTask<bool> ClearAllAsync(CancellationToken cancellationToken)
		{
			return await this.repository.ClearAllAsync(cancellationToken);
		}
		
		#endregion
	}
}