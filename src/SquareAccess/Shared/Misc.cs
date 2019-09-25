using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Xml;
using Money = NMoneys.Money;

namespace SquareAccess.Shared
{
	public static class Misc
	{
		public static string ToJson( this object source )
		{
			try
			{
				if ( source == null )
					return "{}";
				else
				{
					var serialized = JsonConvert.SerializeObject( source, new IsoDateTimeConverter() );
					return serialized;
				}
			}
			catch( Exception )
			{
				return "{}";
			}
		}

		public static string FromUtcToRFC3339( this DateTime dateTimeUtc )
		{
			return XmlConvert.ToString( dateTimeUtc, XmlDateTimeSerializationMode.Utc );
		}

		public static DateTime FromRFC3339ToUtc( this string rfc3339DateTime )
		{
			return XmlConvert.ToDateTime( rfc3339DateTime, XmlDateTimeSerializationMode.Utc );
		}
	}

	public static class NMoneyExtensions
	{
		private static int _moneyDivisor = 100;

		public static Money ToNMoney( this Square.Connect.Model.Money money )
		{
			return money?.Amount != null ? new Money(( decimal ) money.Amount / _moneyDivisor, money.Currency ) : default( Money );
		}
	}
}
