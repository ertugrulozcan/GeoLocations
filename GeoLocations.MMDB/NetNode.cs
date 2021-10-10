namespace GeoLocations.MMDB
{
	internal struct NetNode
	{
		#region Properties

		public byte[] IPBytes { get; set; }
		
		public int Bit { get; set; }
		
		public int Pointer { get; set; }

		#endregion
	}
}