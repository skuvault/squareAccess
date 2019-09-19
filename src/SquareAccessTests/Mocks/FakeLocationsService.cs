﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Model;
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

		public Task< ListLocationsResponse > GetLocationsAsync( CancellationToken token, Mark mark )
		{
			var locations = new ListLocationsResponse( null, new List<Location>
			{
				new Location
				{
					Id = _locationId
				}
			} );

			return Task.FromResult( locations );
		}
	}
}