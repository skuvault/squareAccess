using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;

namespace SquareAccessTests
{
	public class LocationMapperTests
	{
		[ Test ]
		public void ToSvLocation()
		{
			const string locationId = "LKFj432i";
			const string locationName = "Underground Lair";

			var result = new Location( locationId, locationName ).ToSvLocation();

			result.Id.Should().Be( locationId );
			result.Name.Should().Be( locationName );
		}
	}
}
