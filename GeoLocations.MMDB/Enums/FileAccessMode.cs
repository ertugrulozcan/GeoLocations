namespace GeoLocations.MMDB.Enums
{
	public enum FileAccessMode
	{
		/// <summary>
		/// Open the file in memory mapped mode. Does not load into real memory.
		/// </summary>
		MemoryMapped,

		/// <summary>
		/// Open the file in global memory mapped mode. Requires the 'create global objects' right. Does not load into real memory.
		/// </summary>
		/// <remarks>
		/// For information on the 'create global objects' right, see: https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/create-global-objects
		/// </remarks>
		MemoryMappedGlobal,

		/// <summary>
		/// Load the file into memory.
		/// </summary>
		Memory,
	}
}