using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GeoLocations.MMDB.Exceptions;

namespace GeoLocations.MMDB
{
	internal sealed class ArrayBuffer : Buffer
    {
		#region Fields

		private readonly byte[] _fileBytes;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="array"></param>
		public ArrayBuffer(byte[] array)
		{
			this.Length = array.LongLength;
			this._fileBytes = array;
		}
		
		/// <summary>
		/// Constructor 2
		/// </summary>
		/// <param name="file"></param>
		public ArrayBuffer(string file) : this(File.ReadAllBytes(file))
		{
			
		}

		/// <summary>
		/// Constructor 3
		/// </summary>
		/// <param name="stream"></param>
		internal ArrayBuffer(Stream stream) : this(BytesFromStream(stream))
		{
			
		}

		#endregion

		#region Methods

		public static async Task<ArrayBuffer> CreateAsync(string file)
        {
			await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return await CreateAsync(stream).ConfigureAwait(false);
        }

        internal static async Task<ArrayBuffer> CreateAsync(Stream stream)
        {
            return new ArrayBuffer(await BytesFromStreamAsync(stream).ConfigureAwait(false));
        }

        private static byte[] BytesFromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "The database stream must not be null.");
            }

            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            if (bytes.Length == 0)
            {
                throw new InvalidDatabaseException("There are zero bytes left in the stream. Perhaps you need to reset the stream's position.");
            }

            return bytes;
        }

        private static async Task<byte[]> BytesFromStreamAsync(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "The database stream must not be null.");
            }

            byte[] bytes;

			await using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                bytes = memoryStream.ToArray();
            }

            if (bytes.Length == 0)
            {
                throw new InvalidDatabaseException("There are zero bytes left in the stream. Perhaps you need to reset the stream's position.");
            }

            return bytes;
        }

        public override byte[] Read(long offset, int count)
        {
            var bytes = new byte[count];

            if (bytes.Length > 0)
            {
                Array.Copy(this._fileBytes, offset, bytes, 0, bytes.Length);
            }

            return bytes;
        }

        public override byte ReadOne(long offset) => this._fileBytes[offset];

        public override string ReadString(long offset, int count) => Encoding.UTF8.GetString(this._fileBytes, (int)offset, count);

        public override int ReadInt(long offset)
        {
            return 
				this._fileBytes[offset] << 24 |
				this._fileBytes[offset + 1] << 16 |
				this._fileBytes[offset + 2] << 8 |
				this._fileBytes[offset + 3];
        }

        public override int ReadVarInt(long offset, int count)
        {
            return count switch
            {
                0 => 0,
                1 => this._fileBytes[offset],
                2 => this._fileBytes[offset] << 8 | this._fileBytes[offset + 1],
                3 => this._fileBytes[offset] << 16 | this._fileBytes[offset + 1] << 8 | this._fileBytes[offset + 2],
                4 => ReadInt(offset),
                _ => throw new InvalidDatabaseException($"Unexpected int32 of size {count}"),
            };
        }

		#endregion
    }
}