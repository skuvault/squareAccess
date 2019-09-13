using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;
using SquareAccess.Services.Orders;
using SquareAccess.Shared;
using SquareAccessTests.Mocks;

namespace SquareAccessTests
{
	public class OrdersServiceTests : BaseTest
	{
		private ISquareOrdersService _ordersService;
		private bool _firstPage;

		[ SetUp ]
		public void Init()
		{
			this._ordersService = new SquareOrdersService( this.Config, new FakeLocationsService( this.LocationId.Id ) );
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

			result.Count().Should().BeGreaterThan( 2 );
		}

		[ Test ]
		public void CreateSearchOrdersBody()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;
			var locations = new List<Location>
			{
				new Location
				{
					Id = "i2o3jeo"
				},
				new Location
				{
					Id = "aidfj23i"
				}
			};
			const string cursor = "23984ej2";
			const int ordersPerPage = 10;

			var result = SquareOrdersService.CreateSearchOrdersBody( startDateUtc, endDateUtc, locations, cursor, ordersPerPage );

			result.LocationIds.Should().BeEquivalentTo( locations.Select( l => l.Id ) );
			result.Cursor.Should().Be( cursor );
			result.Limit.Should().Be( ordersPerPage );
			result.Query.Filter.DateTimeFilter.UpdatedAt.StartAt.Should().Be( startDateUtc.FromUtcToRFC3339() );
			result.Query.Filter.DateTimeFilter.UpdatedAt.EndAt.Should().Be( endDateUtc.FromUtcToRFC3339() );
		}

		[ Test ]
		public void CollectOrdersFromAllPagesAsync()
		{
			var startDateUtc = new DateTime( 1971, 1, 1 );
			var endDateUtc = DateTime.MaxValue;
			const int ordersPerPage = 2;
			_firstPage = true;
			const string catalogObjectId = "asldfjlkj";
			const string quantity = "13";
			var orders = new List< Order >
			{
				new Order( "alkdsf23", "i2o3jeo" )
				{
					UpdatedAt = "2019-02-03T05:07:51Z",
					LineItems = new List< OrderLineItem >
					{
						new OrderLineItem( null, null, Quantity: quantity )
						{
							CatalogObjectId = catalogObjectId,
						}
					}
				},
				new Order( "23ik4lkj", "aidfj23i" )
				{
					UpdatedAt = "2019-02-03T05:07:51Z"
				}
			};
			var firstCustomer = new SquareCustomer();
			const string sku = "testSku1";
			
			var firstCatalogObjects = new List< CatalogObject >
			{
				new CatalogObject( "asdf", catalogObjectId )
				{
					ItemVariationData = new CatalogItemVariation( "asdfjl", "", sku )
				}
			};

			var result = SquareOrdersService.CollectOrdersFromAllPagesAsync( startDateUtc, endDateUtc, new List< Location >(), 
				( requestBody ) => GetOrdersWithRelatedData( orders, firstCustomer, firstCatalogObjects ), ordersPerPage ).Result.ToList();

			result.Count.Should().Be( 2 );
			var firstOrder = result.First();
			firstOrder.OrderId.Should().BeEquivalentTo( orders.First().Id );
			firstOrder.Customer.Should().BeEquivalentTo( firstCustomer );
			var firstLineItem = firstOrder.LineItems.First();
			firstLineItem.Sku.Should().Be( sku );
			firstLineItem.Quantity.Should().Be( quantity );
			result.Skip( 1 ).First().OrderId.Should().BeEquivalentTo( orders.Skip( 1 ).First().Id );
		}

		private async Task< SquareOrdersBatch > GetOrdersWithRelatedData( IEnumerable< Order > orders, SquareCustomer firstCustomer, IEnumerable< CatalogObject > firstCatalogObjects)
		{
			SquareOrdersBatch result;
			
			if( _firstPage )
			{
				result = new SquareOrdersBatch
				{
					Orders = new List< SquareOrder >
					{
						orders.First().ToSvOrder( firstCustomer, firstCatalogObjects )
					},
					Cursor =  "fas23afs"
				};
			}
			else
			{
				result = new SquareOrdersBatch
				{
					Orders = new List< SquareOrder >
					{
						orders.Skip( 1 ).First().ToSvOrder( null, null )
					}
				};
			}

			_firstPage = false;

			return result;
		}
	}
}
