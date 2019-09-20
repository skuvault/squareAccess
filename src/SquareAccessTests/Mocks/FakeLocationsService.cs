using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Model;
using SquareAccess.Models;
using SquareAccess.Services.Locations;
using SquareAccess.Shared;

namespace SquareAccessTests.Mocks
{
	public class FakeLocationsService : ISquareLocationsService
	{
		private readonly string _locationId;
		
		public FakeLocationsService( string locationId )
		{
			_locationId = locationId;
		}

		public void Dispose()
		{
		}

		public Task< IEnumerable< SquareLocation > > GetActiveLocationsAsync(CancellationToken token, Mark mark)
		{
			return GetLocationsAsync( token, mark );
		}

		public Task< IEnumerable< SquareLocation > > GetLocationsAsync( CancellationToken token, Mark mark )
		{
			var locations = new SquareLocation[] { new SquareLocation() { Id = _locationId, Active = true } }.ToList();

			return Task.FromResult( locations.AsEnumerable() );
		}
	}
}
