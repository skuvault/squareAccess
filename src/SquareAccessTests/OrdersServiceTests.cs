using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SquareAccess.Services.Orders;
using SquareAccessTests.Mocks;

namespace SquareAccessTests
{
	public class OrdersServiceTests : BaseTest
	{
		private ISquareOrdersService _ordersService;

		[ SetUp ]
		public void Init()
		{
			this._ordersService = new SquareOrdersService( this.Config, this.Credentials, new FakeLocationsService( this.LocationId ) );
		}

		[ Test ]
		public void GetOrdersAsync()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;

			var result = _ordersService.GetOrdersAsync( startDateUtc, endDateUtc, CancellationToken.None ).Result;

			result.Should().NotBeEmpty();
		}

		[ Test ]
		public void GetOrdersAsync_ByPage()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;
			this.Config.OrdersPageSize = 2;

			var result = _ordersService.GetOrdersAsync( startDateUtc, endDateUtc, CancellationToken.None ).Result;

			result.Count().Should().BeGreaterThan(2);
		}

		//TODO Test CreateSearchOrdersBody
		//TODO Test CollectOrdersFromAllPagesAsync
		//TODO Test ToSvOrder
	}
}
