using System.Net;

namespace GeoLocations.MMDB
{
	public sealed class Network
	{
		#region Fields

		private readonly IPAddress ip;

		#endregion
		
		#region Properties

		/// <summary>
		/// The prefix length is the number of leading 1 bits in the subnet mask. Sometimes also known as netmask length.
		/// </summary>
		public int PrefixLength { get; }
		
		/// <summary>
		/// The first address in the network.
		/// </summary>
		public IPAddress NetworkAddress
		{
			get
			{
				var ipBytes = ip.GetAddressBytes();
				var networkBytes = new byte[ipBytes.Length];
				
				var curPrefix = this.PrefixLength;
				for (var i = 0; i < ipBytes.Length && curPrefix > 0; i++)
				{
					var b = ipBytes[i];
					if (curPrefix < 8)
					{
						var shiftN = 8 - curPrefix;
						b = (byte)(0xFF & (b >> shiftN) << shiftN);
					}
					
					networkBytes[i] = b;
					curPrefix -= 8;
				}

				return new IPAddress(networkBytes);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="prefixLength"></param>
		public Network(IPAddress ip, int prefixLength)
		{
			this.ip = ip;
			this.PrefixLength = prefixLength;
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return $"{NetworkAddress}/{PrefixLength}";
		}

		#endregion
	}
}