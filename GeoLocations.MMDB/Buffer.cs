using System;
using System.Numerics;

namespace GeoLocations.MMDB
{
	internal abstract class Buffer : IDisposable
    {
		#region Properties

		public long Length { get; protected set; }

		#endregion

		#region Abstract Methods

		public abstract byte[] Read(long offset, int count);

		public abstract string ReadString(long offset, int count);

		public abstract int ReadInt(long offset);

		public abstract int ReadVarInt(long offset, int count);

		public abstract byte ReadOne(long offset);

		#endregion

		#region Methods

		internal BigInteger ReadBigInteger(long offset, int size)
		{
			// This could be optimized if it ever matters
			var buffer = this.Read(offset, size);
			Array.Reverse(buffer);

			// The integer will always be positive. We need to make sure the last bit is 0.
			// ReSharper disable once UseIndexFromEndExpression
			if (buffer.Length > 0 && (buffer[buffer.Length - 1] & 0x80) > 0)
			{
				Array.Resize(ref buffer, buffer.Length + 1);
			}
			
			return new BigInteger(buffer);
		}

		internal double ReadDouble(long offset)
		{
			return BitConverter.Int64BitsToDouble(this.ReadLong(offset, 8));
		}
		
		internal float ReadFloat(long offset)
		{
#if NETSTANDARD2_0 || NET461
            var buffer = Read(offset, 4);
            Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
#else
			return BitConverter.Int32BitsToSingle(ReadInt(offset));
#endif
		}
		
		internal long ReadLong(long offset, int size)
		{
			long val = 0;
			for (var i = 0; i < size; i++)
			{
				val = (val << 8) | this.ReadOne(offset + i);
			}
			
			return val;
		}
		
		internal ulong ReadULong(long offset, int size)
		{
			ulong val = 0;
			for (var i = 0; i < size; i++)
			{
				val = (val << 8) | this.ReadOne(offset + i);
			}
			
			return val;
		}
		
		#endregion

		#region Disposing

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// This is overridden in subclasses.
		}

		#endregion
    }
}