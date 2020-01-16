using CsvHelper;
using CsvHelper.Configuration;
using SquareAccess.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SquareAccess.Models;

namespace SquareAccessTests
{
	public class BaseTest
	{
		protected SquareConfig Config { get; private set; }
		protected SquareMerchantCredentials Credentials { get; private set; }

		public BaseTest()
		{
			var config = this.LoadTestSettings< SquareCredentials >( @"\..\..\credentials.csv" );
			
			this.Config = new SquareConfig( config.ApplicationId, config.ApplicationSecret );
			this.Credentials = new SquareMerchantCredentials( config.AccessToken, config.RefreshToken );
		}

		protected T LoadTestSettings< T >( string filePath )
		{
			string basePath = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var streamReader = new StreamReader( basePath + filePath ) )
			{
				var csvConfig = new Configuration()
				{
					Delimiter = ","
				};

				using( var csvReader = new CsvReader( streamReader, csvConfig ) )
				{
					var credentials = csvReader.GetRecords< T >();

					return credentials.FirstOrDefault();
				}
			}
		}
	}
}
