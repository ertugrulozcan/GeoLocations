using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace GeoLocations.MMDB.Exceptions
{
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[Serializable]
	public sealed class DeserializationException : Exception
	{
		#region Constructors

		/// <summary>
		/// Constructor 1
		/// </summary>
		/// <param name="message"></param>
		public DeserializationException(string message) : base(message)
		{
			
		}
		
		/// <summary>
		/// Constructor 2
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public DeserializationException(string message, Exception innerException) : base(message, innerException)
		{
			
		}
		
		/// <summary>
		/// Constructor 3
		/// </summary>
		/// <param name="info">The SerializationInfo with data.</param>
		/// <param name="context">The source for this deserialization.</param>
		private DeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			
		}

		#endregion
	}
}