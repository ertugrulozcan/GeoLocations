namespace GeoLocations.Core.Models
{
	public class LocalizedVariable<T> where T : notnull
	{
		#region Properties

		public string LanguageCode { get; set; }
		
		public T Value { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		public LocalizedVariable()
		{
			this.LanguageCode = null;
			this.Value = default;
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="languageCode"></param>
		/// <param name="value"></param>
		public LocalizedVariable(string languageCode, T value)
		{
			this.LanguageCode = languageCode;
			this.Value = value;
		}

		#endregion
	}
}