using System.Net;

namespace GeoLocations.MMDB
{
	/// <summary>
	/// A node from the reader iterator
	/// </summary>
	public readonly struct ReaderIteratorNode<T>
	{
		#region Properties

		/// <summary>
		/// Start ip address
		/// </summary>
		public IPAddress Start { get; }

		/// <summary>
		/// Prefix/mask length
		/// </summary>
		public int PrefixLength { get; }

		/// <summary>
		/// Data
		/// </summary>
		public T Data { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <param name="start">Start ip</param>
		/// <param name="prefixLength">Prefix length</param>
		/// <param name="data">Data</param>
		internal ReaderIteratorNode(IPAddress start, int prefixLength, T data)
		{
			Start = start;
			PrefixLength = prefixLength;
			Data = data;
		}

		#endregion
	}
}