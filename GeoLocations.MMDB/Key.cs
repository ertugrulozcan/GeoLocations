namespace GeoLocations.MMDB
{
	internal readonly struct Key
	{
		#region Fields

		private readonly Buffer buffer;
		private readonly long offset;
		private readonly int size;
		private readonly int hashCode;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		public Key(Buffer buffer, long offset, int size)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.size = size;

			var code = 17;
			for (var i = 0; i < size; i++)
			{
				code = 31 * code + buffer.ReadOne(offset + i);
			}
			
			this.hashCode = code;
		}

		#endregion

		#region Methods

		public override bool Equals(object? obj)
		{
			// ReSharper disable once CheckForReferenceEqualityInstead.1
			// ReSharper disable once CheckForReferenceEqualityInstead.3
			if ((obj == null) || !GetType().Equals(obj.GetType()))
			{
				return false;
			}

			var other = (Key)obj;
			if (this.size != other.size)
			{
				return false;
			}

			for (var i = 0; i < size; i++)
			{
				if (this.buffer.ReadOne(this.offset + i) != other.buffer.ReadOne(other.offset + i))
				{
					return false;
				}
			}
			
			return true;
		}

		public override int GetHashCode()
		{
			return hashCode;
		}

		#endregion
	}
}