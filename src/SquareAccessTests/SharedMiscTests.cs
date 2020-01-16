using System;
using NUnit.Framework;
using SquareAccess.Shared;

namespace SquareAccessTests
{
	[ TestFixture ]
	public class SharedMiscTests
	{
		[ Test ]
		public void FromUtcToRFC3339()
		{
			const int year = 2019;
			const int month = 2;
			const int day = 3;
			const int hour = 5;
			const int minute = 7;
			const int second = 51;
			var dateTimeUtc = new DateTime( year, month, day, hour, minute, second, DateTimeKind.Utc ); 

			var dateTimeRFC3339 = dateTimeUtc.FromUtcToRFC3339();

			Assert.AreEqual( $"{year:D4}-{month:D2}-{day:D2}T{hour:D2}:{minute:D2}:{second:D2}Z", dateTimeRFC3339 );
		}

		[ Test ]
		public void FromRFC3339ToUtc()
		{
			const int year = 2019;
			const int month = 2;
			const int day = 3;
			const int hour = 5;
			const int minute = 7;
			const int second = 51;
			var rfc3339DateTime = $"{year:D4}-{month:D2}-{day:D2}T{hour:D2}:{minute:D2}:{second:D2}Z";

			var dateTimeUtc = rfc3339DateTime.FromRFC3339ToUtc();

			Assert.AreEqual( new DateTime( year, month, day, hour, minute, second, DateTimeKind.Utc ), dateTimeUtc );
		}
	}
}
