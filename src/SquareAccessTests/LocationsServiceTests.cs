﻿using System.Linq;
using System.Threading;
using NUnit.Framework;
using SquareAccess.Services.Locations;
using SquareAccess.Shared;

namespace SquareAccessTests
{
	public class LocationsServiceTests : BaseTest
	{
		private ISquareLocationsService _locationsService;

		[ SetUp ]
		public void Init()
		{
			this._locationsService = new SquareLocationsService( this.Config, this.Credentials );
		}

		[ Test ]
		public void GetLocationsAsync()
		{
			var locations = _locationsService.GetActiveLocationsAsync( CancellationToken.None, Mark.CreateNew() ).Result;

			Assert.IsNotNull( locations );
			Assert.AreNotEqual( 0, locations.Count() );
		}
	}
}
