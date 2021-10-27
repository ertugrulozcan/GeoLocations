using System.Collections.Generic;
using System.Linq;
using GeoLocations.Core.Models;
using GeoLocations.Dao.Models;

namespace GeoLocations.Dao.Extensions
{
	public static class EntityExtensions
	{
		#region GeoLocation

		public static GeoLocation ToModel(this GeoLocationEntity entity)
		{
			return new GeoLocation
			{
				Id = entity.Id,
				IP = entity.IP,
				IpVersion = entity.IpVersion,
				City = entity.City?.ToModel(),
				Country = entity.Country?.ToModel(),
				Continent = entity.Continent?.ToModel(),
				County = entity.County?.ToModel(),
				Region = entity.Region?.ToModel(),
				Currency = entity.Currency,
				Location = new Coordinate
				{
					Latitude = entity.Latitude,
					Longitude = entity.Longitude
				},
				Organization = entity.Organization,
				TimeZone = entity.TimeZone,
				ZipCode = entity.ZipCode,
				ISP = entity.ISP,
				WeatherStationCode = entity.WeatherStationCode
			};
		}
		
		public static GeoLocationEntity ToEntity(this GeoLocation model)
		{
			return new GeoLocationEntity
			{
				Id = model.Id,
				IP = model.IP,
				IpVersion = model.IpVersion,
				City = model.City?.ToEntity(),
				Country = model.Country?.ToEntity(),
				Continent = model.Continent?.ToEntity(),
				County = model.County?.ToEntity(),
				Region = model.Region?.ToEntity(),
				Currency = model.Currency,
				Latitude = model.Location?.Latitude,
				Longitude = model.Location?.Longitude,
				Organization = model.Organization,
				TimeZone = model.TimeZone,
				ZipCode = model.ZipCode,
				ISP = model.ISP,
				WeatherStationCode = model.WeatherStationCode
			};
		}

		#endregion
		
		#region City

		public static City ToModel(this CityEntity entity)
		{
			return new City
			{
				Names = entity.Names?.Select(x => new LocalizedVariable<string>(x.Locale, x.Name))
			};
		}
		
		public static CityEntity ToEntity(this City model)
		{
			var entity = GenerateLocalizedEntity(model.Names.ToArray(), CityDictionary);
			entity.Names = model.Names.Select(x => new CityNamesEntity { City = entity, Locale = x.LanguageCode, Name = x.Value }).ToArray();
			return entity;
		}

		#endregion

		#region Country

		public static Country ToModel(this CountryEntity entity)
		{
			return new Country
			{
				Names = entity.Names?.Select(x => new LocalizedVariable<string>(x.Locale, x.Name)),
				PhoneCode = entity.PhoneCode,
				GeoNameId = entity.GeoNameId,
				IsInEuropeanUnion = entity.IsInEuropeanUnion,
				ISOCode = entity.ISOCode
			};
		}
		
		public static CountryEntity ToEntity(this Country model)
		{
			var entity = GenerateLocalizedEntity(model.Names.ToArray(), CountryDictionary);
			entity.ISOCode = model.ISOCode;
			entity.PhoneCode = model.PhoneCode;
			entity.GeoNameId = model.GeoNameId;
			entity.IsInEuropeanUnion = model.IsInEuropeanUnion;
			entity.Names = model.Names.Select(x => new CountryNamesEntity { Country = entity, Locale = x.LanguageCode, Name = x.Value }).ToArray();
			return entity;
		}

		#endregion
		
		#region Continent

		public static Continent ToModel(this ContinentEntity entity)
		{
			return new Continent
			{
				Names = entity.Names?.Select(x => new LocalizedVariable<string>(x.Locale, x.Name)),
				GeoNameId = entity.GeoNameId,
				Code = entity.Code
			};
		}
		
		public static ContinentEntity ToEntity(this Continent model)
		{
			var entity = GenerateLocalizedEntity(model.Names.ToArray(), ContinentDictionary);
			entity.Code = model.Code;
			entity.GeoNameId = model.GeoNameId;
			entity.Names = model.Names.Select(x => new ContinentNamesEntity { Continent = entity, Locale = x.LanguageCode, Name = x.Value }).ToArray();
			return entity;
		}

		#endregion
		
		#region County

		public static County ToModel(this CountyEntity entity)
		{
			return new County
			{
				Names = entity.Names?.Select(x => new LocalizedVariable<string>(x.Locale, x.Name))
			};
		}
		
		public static CountyEntity ToEntity(this County model)
		{
			var entity = GenerateLocalizedEntity(model.Names.ToArray(), CountyDictionary);
			entity.Names = model.Names.Select(x => new CountyNamesEntity { County = entity, Locale = x.LanguageCode, Name = x.Value }).ToArray();
			return entity;
		}

		#endregion
		
		#region Region

		public static Region ToModel(this RegionEntity entity)
		{
			return new Region
			{
				Names = entity.Names?.Select(x => new LocalizedVariable<string>(x.Locale, x.Name))
			};
		}
		
		public static RegionEntity ToEntity(this Region model)
		{
			var entity = GenerateLocalizedEntity(model.Names.ToArray(), RegionDictionary);
			entity.Names = model.Names.Select(x => new RegionNamesEntity { Region = entity, Locale = x.LanguageCode, Name = x.Value }).ToArray();
			return entity;
		}

		#endregion
		
		#region Localized Entity Ids

		private static readonly Dictionary<string, CityEntity> CityDictionary = new();
		private static readonly Dictionary<string, CountryEntity> CountryDictionary = new();
		private static readonly Dictionary<string, ContinentEntity> ContinentDictionary = new();
		private static readonly Dictionary<string, CountyEntity> CountyDictionary = new();
		private static readonly Dictionary<string, RegionEntity> RegionDictionary = new();

		private static T GenerateLocalizedEntity<T>(IList<LocalizedVariable<string>> names, IDictionary<string, T> dictionary) where T : EntityBase, new()
		{
			if (names != null && names.Any())
			{
				var englishName = names.FirstOrDefault(x => x.LanguageCode == "en");
				var localizedName = englishName ?? names.First();
				var key = $"{localizedName.LanguageCode}:{localizedName.Value}";
				if (dictionary.ContainsKey(key))
				{
					return dictionary[key];
				}
				else
				{
					var entity = new T
					{
						Id = dictionary.Count + 1
					};
					
					dictionary.Add(key, entity);
					return entity;
				}
			}

			return default;
		}

		#endregion
	}
}