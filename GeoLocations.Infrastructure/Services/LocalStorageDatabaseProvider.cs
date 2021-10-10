using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GeoLocations.Abstractions.Services;
using GeoLocations.Core.Models;
using GeoLocations.MMDB;

namespace GeoLocations.Infrastructure.Services
{
	public class LocalStorageDatabaseProvider : IMasterDatabaseProvider
	{
		#region Properties

		/// <summary>
		/// File path for MMDB file
		/// </summary>
		public string FilePath { get; } = "../dbip-city-lite-2021-09.mmdb";

		#endregion
		
		#region Methods

		public byte[] GetBinaryDatabase()
		{
			return File.ReadAllBytes(this.FilePath);
		}

		private static IEnumerable<LocalizedVariable<string>> GetLocalizations(IDictionary<string, object> dictionary)
		{
			foreach (var (languageCode, nameNode) in dictionary)
			{
				if (nameNode != null)
				{
					yield return new LocalizedVariable<string>(languageCode, nameNode.ToString() ?? string.Empty);
				}
			}
		}

		public IEnumerable<GeoLocation> GetData(int? skip = null!, int? limit = null!)
		{
			int counter = 0;
			
			using (var reader = new Reader(this.FilePath))
			{
				var nodes = reader.FindAll<Dictionary<string, object>>();
				foreach (var node in nodes)
				{
					if (counter < skip)
					{
						counter++;
						continue;
					}

					var geoLocation = new GeoLocation
					{
						IP = node.Start.ToString(),
						IpVersion = IpVersion.Version4
					};

					// City
					if (node.Data.ContainsKey("city"))
					{
						geoLocation.City = new City();
						
						var cityNode = node.Data["city"];
						if (cityNode is IDictionary<string, object> cityDictionary)
						{
							if (cityDictionary.ContainsKey("names"))
							{
								var names = cityDictionary["names"];
								if (names is IDictionary<string, object> namesDictionary)
								{
									geoLocation.City.Names = GetLocalizations(namesDictionary);
								}
							}
						}
					}
					
					// Country
					if (node.Data.ContainsKey("country"))
					{
						geoLocation.Country = new Country();
						
						var countryNode = node.Data["country"];
						if (countryNode is IDictionary<string, object> countryDictionary)
						{
							// GeoNameId
							if (countryDictionary.ContainsKey("geoname_id"))
							{
								var geoNameIdNode = countryDictionary["geoname_id"];
								if (geoNameIdNode != null && long.TryParse(geoNameIdNode.ToString(), out long geoNameId))
								{
									geoLocation.Country.GeoNameId = geoNameId;
								}
							}
							
							// IsInEuropeanUnion
							if (countryDictionary.ContainsKey("is_in_european_union"))
							{
								var isInEuropeanUnionNode = countryDictionary["is_in_european_union"];
								if (isInEuropeanUnionNode != null && bool.TryParse(isInEuropeanUnionNode.ToString(), out bool isInEuropeanUnion))
								{
									geoLocation.Country.IsInEuropeanUnion = isInEuropeanUnion;
								}
							}
							
							// IsoCode
							if (countryDictionary.ContainsKey("iso_code"))
							{
								var isoCodeNode = countryDictionary["iso_code"];
								if (isoCodeNode != null)
								{
									geoLocation.Country.ISOCode = isoCodeNode.ToString();
								}
							}
							
							// Country Name
							if (countryDictionary.ContainsKey("names"))
							{
								var names = countryDictionary["names"];
								if (names is IDictionary<string, object> namesDictionary)
								{
									geoLocation.Country.Names = GetLocalizations(namesDictionary);
								}
							}
						}
					}
					
					// Continent
					if (node.Data.ContainsKey("continent"))
					{
						geoLocation.Continent = new Continent();
						
						var continentNode = node.Data["continent"];
						if (continentNode is IDictionary<string, object> continentDictionary)
						{
							// GeoNameId
							if (continentDictionary.ContainsKey("geoname_id"))
							{
								var geoNameIdNode = continentDictionary["geoname_id"];
								if (geoNameIdNode != null && long.TryParse(geoNameIdNode.ToString(), out long geoNameId))
								{
									geoLocation.Continent.GeoNameId = geoNameId;
								}
							}

							// Code
							if (continentDictionary.ContainsKey("code"))
							{
								var continentCodeNode = continentDictionary["code"];
								if (continentCodeNode != null)
								{
									geoLocation.Continent.Code = continentCodeNode.ToString();
								}
							}
							
							// Country Name
							if (continentDictionary.ContainsKey("names"))
							{
								var names = continentDictionary["names"];
								if (names is IDictionary<string, object> namesDictionary)
								{
									geoLocation.Continent.Names = GetLocalizations(namesDictionary);
								}
							}
						}
					}
					
					// Location
					if (node.Data.ContainsKey("location"))
					{
						double? latitude = null;
						double? longitude = null;

						var locationNode = node.Data["location"];
						if (locationNode is IDictionary<string, object> locationDictionary)
						{
							// Latitude
							if (locationDictionary.ContainsKey("latitude"))
							{
								var latitudeNode = locationDictionary["latitude"];
								if (latitudeNode != null && double.TryParse(latitudeNode.ToString(), out double latitude_))
								{
									latitude = latitude_;
								}
							}
							
							// Longitude
							if (locationDictionary.ContainsKey("longitude"))
							{
								var longitudeNode = locationDictionary["longitude"];
								if (longitudeNode != null && double.TryParse(longitudeNode.ToString(), out double longitude_))
								{
									longitude = longitude_;
								}
							}
						}

						geoLocation.Location = new Coordinate
						{
							Latitude = latitude,
							Longitude = longitude
						};
					}
					
					// Subdivisions
					if (node.Data.ContainsKey("subdivisions"))
					{
						geoLocation.Region = new Region();
						
						var subdivisionsNode = node.Data["subdivisions"];
						if (subdivisionsNode is IList<object> subdivisionsList)
						{
							foreach (var subdivisionObject in subdivisionsList)
							{
								if (subdivisionObject is IDictionary<string, object> subdivisionsDictionary)
								{
									if (subdivisionsDictionary.ContainsKey("names"))
									{
										var names = subdivisionsDictionary["names"];
										if (names is IDictionary<string, object> namesDictionary)
										{
											geoLocation.Region.Names = GetLocalizations(namesDictionary);
										}
									}
								}
							}
						}
					}
					
					counter++;

					if (counter > limit)
					{
						break;
					}

					yield return geoLocation;
				}
			}
		}
		
		public async Task<IEnumerable<GeoLocation>> GetDataAsync(int? skip = null!, int? limit = null!)
		{
			return await Task.Run(() => this.GetData(skip, limit));
		}

		#endregion
	}
}