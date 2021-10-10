using System;
using System.Collections.Generic;
using GeoLocations.MMDB.Attributes;

namespace GeoLocations.MMDB
{
	public sealed class Metadata
    {
		#region Properties

		/// <summary>
		/// The major version number for the DB binary format used by the database.
		/// </summary>
		public int BinaryFormatMajorVersion { get; }

		/// <summary>
		/// The minor version number for the DB binary format used by the database.
		/// </summary>
		public int BinaryFormatMinorVersion { get; }

		/// <summary>
		/// BuildEpoch
		/// </summary>
		internal ulong BuildEpoch { get; }

		/// <summary>
		/// The date-time of the database build.
		/// </summary>
		public DateTime BuildDate => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(BuildEpoch);

		/// <summary>
		/// The DB database type.
		/// </summary>
		public string DatabaseType { get; }

		/// <summary>
		/// A map from locale codes to the database description in that language.
		/// </summary>
		public IDictionary<string, string> Description { get; }

		/// <summary>
		/// The IP version that the database supports. This will be 4 or 6.
		/// </summary>
		public int IPVersion { get; }

		/// <summary>
		/// A list of locale codes for languages that the database supports.
		/// </summary>
		public IReadOnlyList<string> Languages { get; }

		/// <summary>
		/// NodeCount
		/// </summary>
		internal long NodeCount { get; }

		/// <summary>
		/// RecordSize
		/// </summary>
		internal int RecordSize { get; }

		/// <summary>
		/// NodeByteSize
		/// </summary>
		internal long NodeByteSize => RecordSize / 4;

		/// <summary>
		/// SearchTreeSize
		/// </summary>
		internal long SearchTreeSize => NodeCount * NodeByteSize;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct
		/// </summary>
		/// <param name="binaryFormatMajorVersion"></param>
		/// <param name="binaryFormatMinorVersion"></param>
		/// <param name="buildEpoch"></param>
		/// <param name="databaseType"></param>
		/// <param name="description"></param>
		/// <param name="ipVersion"></param>
		/// <param name="languages"></param>
		/// <param name="nodeCount"></param>
		/// <param name="recordSize"></param>
		[Constructor]
		public Metadata(
			[Parameter("binary_format_major_version")] int binaryFormatMajorVersion,
			[Parameter("binary_format_minor_version")] int binaryFormatMinorVersion,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
			[Parameter("build_epoch")] ulong buildEpoch,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
			[Parameter("database_type")] string databaseType,
			IDictionary<string, string> description,
			[Parameter("ip_version")] int ipVersion,
			IReadOnlyList<string> languages,
			[Parameter("node_count")] long nodeCount,
			[Parameter("record_size")] int recordSize)
		{
			this.BinaryFormatMajorVersion = binaryFormatMajorVersion;
			this.BinaryFormatMinorVersion = binaryFormatMinorVersion;
			this.BuildEpoch = buildEpoch;
			this.DatabaseType = databaseType;
			this.Description = description;
			this.IPVersion = ipVersion;
			this.Languages = languages;
			this.NodeCount = nodeCount;
			this.RecordSize = recordSize;
		}

		#endregion
    }
}