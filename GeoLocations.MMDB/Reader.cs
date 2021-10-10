using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GeoLocations.MMDB.Enums;
using GeoLocations.MMDB.Exceptions;

namespace GeoLocations.MMDB
{
	public sealed class Reader : IDisposable
    {
		#region Constants

		private const int DataSectionSeparatorSize = 16;

		#endregion
		
		#region Fields

		private int _ipV4Start;
		private readonly Buffer _database;
		private readonly string? _fileName;

		private readonly byte[] _metadataStartMarker =
		{
			0xAB, 0xCD, 0xEF, 77, 97, 120, 77, 105, 110, 100, 46, 99, 111, 109
		};

		#endregion
		
		#region Properties

		public Metadata Metadata { get; }
		
		private Decoder Decoder { get; }

		private int IPv4Start
		{
			get
			{
				if (this._ipV4Start != 0 || this.Metadata.IPVersion == 4)
				{
					return this._ipV4Start;
				}
				
				var node = 0;
				for (var i = 0; i < 96 && node < this.Metadata.NodeCount; i++)
				{
					node = this.ReadNode(node, 0);
				}
				
				this._ipV4Start = node;
				return node;
			}
		}

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor 1
		/// </summary>
		/// <param name="file"></param>
		public Reader(string file) : this(file, FileAccessMode.MemoryMapped)
		{
			
		}

		/// <summary>
		/// Constructor 2
		/// </summary>
		/// <param name="file"></param>
		/// <param name="mode"></param>
		public Reader(string file, FileAccessMode mode) : this(BufferForMode(file, mode), file)
		{
			
		}

		/// <summary>
		/// Constructor 3
		/// </summary>
		/// <param name="stream"></param>
		public Reader(Stream stream) : this(new ArrayBuffer(stream), null)
		{
			
		}

		/// <summary>
		/// Constructor 4
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="file"></param>
		private Reader(Buffer buffer, string? file)
		{
			this._fileName = file;
			this._database = buffer;
			
			var start = this.FindMetadataStart();
			var metaDecode = new Decoder(this._database, start);
			this.Metadata = metaDecode.Decode<Metadata>(start, out _);
			this.Decoder = new Decoder(this._database, this.Metadata.SearchTreeSize + DataSectionSeparatorSize);
		}

		#endregion
		
		#region Methods

		public static async Task<Reader> CreateAsync(string file)
		{
			return new Reader(await ArrayBuffer.CreateAsync(file).ConfigureAwait(false), file);
		}

		public static async Task<Reader> CreateAsync(Stream stream)
		{
			return new Reader(await ArrayBuffer.CreateAsync(stream).ConfigureAwait(false), null);
		}

		private static Buffer BufferForMode(string file, FileAccessMode mode)
		{
			return mode switch
			{
				FileAccessMode.MemoryMapped => new MemoryMapBuffer(file, false),
				FileAccessMode.MemoryMappedGlobal => new MemoryMapBuffer(file, true),
				FileAccessMode.Memory => new ArrayBuffer(file),
				_ => throw new ArgumentException("Unknown file access mode"),
			};
		}
		
		/// <summary>
		/// Finds the data related to the specified address.
		/// </summary>
		/// <param name="ipAddress">The IP address.</param>
		/// <param name="injectables">Value to inject during deserialization</param>
		/// <returns>An object containing the IP related data</returns>
		public T? Find<T>(string ipAddress, InjectableValues? injectables = null) where T : class
		{
			return Find<T>(IPAddress.Parse(ipAddress), out _, injectables);
		}
		
		/// <summary>
        /// Finds the data related to the specified address.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="injectables">Value to inject during deserialization</param>
        /// <returns>An object containing the IP related data</returns>
        public T? Find<T>(IPAddress ipAddress, InjectableValues? injectables = null) where T : class
        {
            return Find<T>(ipAddress, out _, injectables);
        }

        /// <summary>
        /// Finds the data related to the specified address.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="prefixLength">The network prefix length for the network record in the database containing the IP address looked up.</param>
        /// <param name="injectables">Value to inject during deserialization</param>
        /// <returns>An object containing the IP related data</returns>
        public T? Find<T>(IPAddress ipAddress, out int prefixLength, InjectableValues? injectables = null) where T : class
        {
            var pointer = FindAddressInTree(ipAddress, out prefixLength);
            var network = new Network(ipAddress, prefixLength);
            return pointer == 0 ? null : ResolveDataPointer<T>(pointer, injectables, network);
        }

        /// <summary>
        /// Get an enumerator that iterates all data nodes in the database. Do not modify the object as it may be cached.
		/// Note that due to caching, the Network attribute on constructor parameters will be ignored.
        /// </summary>
        /// <param name="injectables">Value to inject during deserialization</param>
        /// <param name="cacheSize">The size of the data cache. This can greatly speed enumeration at the cost of memory usage.</param>
        /// <returns>Enumerator for all data nodes</returns>
        public IEnumerable<ReaderIteratorNode<T>> FindAll<T>(InjectableValues? injectables = null, int cacheSize = 16384) where T : class
        {
            var byteCount = Metadata.IPVersion == 6 ? 16 : 4;
            var nodes = new List<NetNode>();
            var root = new NetNode { IPBytes = new byte[byteCount] };
            nodes.Add(root);
			
            var dataCache = new CachedDictionary<int, T>(cacheSize, null);
            while (nodes.Count > 0)
            {
				// ReSharper disable once UseIndexFromEndExpression
				var node = nodes[nodes.Count - 1];
                nodes.RemoveAt(nodes.Count - 1);
                while (true)
                {
                    if (node.Pointer < Metadata.NodeCount)
                    {
                        var ipRight = new byte[byteCount];
                        Array.Copy(node.IPBytes, ipRight, ipRight.Length);
                        if (ipRight.Length <= node.Bit >> 3)
                        {
                            throw new InvalidDataException("Invalid search tree, bad bit " + node.Bit);
                        }
						
                        ipRight[node.Bit >> 3] |= (byte)(1 << (7 - (node.Bit % 8)));
                        var rightPointer = ReadNode(node.Pointer, 1);
                        node.Bit++;
                        nodes.Add(new NetNode { Pointer = rightPointer, IPBytes = ipRight, Bit = node.Bit });
                        node.Pointer = ReadNode(node.Pointer, 0);
                    }
                    else
                    {
                        if (node.Pointer > Metadata.NodeCount)
                        {
                            // data node, we are done with this branch
                            if (!dataCache.TryGetValue(node.Pointer, out var data))
                            {
                                data = ResolveDataPointer<T>(node.Pointer, injectables, null);
                                dataCache.Add(node.Pointer, data);
                            }
							
                            var isIPV4 = true;
                            for (var i = 0; i < node.IPBytes.Length - 4; i++)
                            {
                                if (node.IPBytes[i] == 0) continue;

                                isIPV4 = false;
                                break;
                            }
							
                            if (!isIPV4 || node.IPBytes.Length == 4)
                            {
                                yield return new ReaderIteratorNode<T>(new IPAddress(node.IPBytes), node.Bit, data);
                            }
                            else
                            {
                                yield return new ReaderIteratorNode<T>(new IPAddress(node.IPBytes.Skip(12).Take(4).ToArray()), node.Bit - 96, data);
                            }
                        }
						
                        // else node is an empty node (terminator node), we are done with this branch
                        break;
                    }
                }
            }
        }

		private T ResolveDataPointer<T>(int pointer, InjectableValues? injectables, Network? network) where T : class
        {
            var resolved = pointer - this.Metadata.NodeCount + this.Metadata.SearchTreeSize;
			if (resolved >= this._database.Length)
            {
                throw new InvalidDatabaseException("The Db file's search tree is corrupt: contains pointer larger than the database.");
            }

            return this.Decoder.Decode<T>(resolved, out _, injectables, network);
        }

        private int FindAddressInTree(IPAddress address, out int prefixLength)
        {
            var rawAddress = address.GetAddressBytes();
			var bitLength = rawAddress.Length * 8;
            var record = this.StartNode(bitLength);
            var nodeCount = this.Metadata.NodeCount;

            var i = 0;
            for (; i < bitLength && record < nodeCount; i++)
            {
                var bit = 1 & (rawAddress[i >> 3] >> (7 - (i % 8)));
                record = ReadNode(record, bit);
            }
			
            prefixLength = i;
            if (record == this.Metadata.NodeCount)
            {
                // record is empty
                return 0;
            }
			
            if (record > this.Metadata.NodeCount)
            {
                // record is a data pointer
                return record;
            }
			
            throw new InvalidDatabaseException("Something bad happened");
        }

        private int StartNode(int bitLength)
        {
            // Check if we are looking up an IPv4 address in an IPv6 tree. If this is the case, we can skip over the first 96 nodes.
            if (this.Metadata.IPVersion == 6 && bitLength == 32)
            {
                return this.IPv4Start;
            }
			
            // The first node of the tree is always node 0, at the beginning of the value
            return 0;
        }

        private long FindMetadataStart()
        {
            var dbLength = this._database.Length;
            var markerLength = (long) this._metadataStartMarker.Length;

            for (var i = dbLength - markerLength; i > 0; i--) 
			{
                int j = 0;
                for (; j < markerLength; j++)
                {
                    if (this._metadataStartMarker[j] != this._database.ReadOne(i + j))
                    {
                        break;
                    }
                }
				
                if (j == markerLength)
                {
                    return i + markerLength;
                }
            }

            throw new InvalidDatabaseException($"Could not find a DB metadata marker in this file ({_fileName}). Is this a valid DB file?");
        }

        private int ReadNode(int nodeNumber, int index)
        {
            var baseOffset = nodeNumber * this.Metadata.NodeByteSize;
			var size = this.Metadata.RecordSize;

            switch (size)
            {
                case 24:
                    {
                        var offset = baseOffset + index * 3;
                        return this._database.ReadVarInt(offset, 3);
                    }
                case 28:
                    {
                        if (index == 0)
                        {
                            var v = this._database.ReadInt(baseOffset);
                            return (v & 0xF0) << 20 | (0xFFFFFF & (v >> 8));
                        }
                        return this._database.ReadInt(baseOffset + 3) & 0x0FFFFFFF;
                    }
                case 32:
                    {
                        var offset = baseOffset + index * 4;
                        return this._database.ReadInt(offset);
                    }
            }

            throw new InvalidDatabaseException($"Unknown record size: {size}");
        }

		#endregion
		
		#region Disposing

		private bool _disposed;
		
		public void Dispose()
		{
			this.Dispose(true);
			
			// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this._disposed)
				return;

			if (disposing)
			{
				this._database.Dispose();
			}

			this._disposed = true;
		}

		#endregion
    }
}