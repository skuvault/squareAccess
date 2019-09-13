using CsvHelper;
using CsvHelper.Configuration;
using SquareAccess.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SquareAccessTests
{
	public class BaseTest
	{
		protected SquareConfig Config { get; private set; }
		protected SquareCredentials Credentials { get; private set; }

		public BaseTest()
		{
			this.Credentials = this.LoadCredentials( @"\..\..\credentials.csv" );
			this.Config = new SquareConfig( this.Credentials.ApplicationId, this.Credentials.ApplicationSecret );
		}

		protected SquareCredentials LoadCredentials( string filePath )
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
					var credentials = csvReader.GetRecords< SquareCredentials >();

					return credentials.FirstOrDefault();
				}
			}
		}
	}
}
