using System;

namespace GeoLocations.PostgreSQL.Repositories
{
	public class RepositoryContextFactory : IRepositoryContextFactory
	{
		#region Services

		private readonly IServiceProvider serviceProvider;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="serviceProvider"></param>
		public RepositoryContextFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		#endregion
		
		#region Methods

		public RepositoryContext ResolveRepositoryContext()
		{
			var instance = this.serviceProvider.GetService(typeof(RepositoryContext));
			if (instance is RepositoryContext repositoryContext)
			{
				return repositoryContext;
			}
			else
			{
				return default;
			}
		}

		#endregion
	}
}