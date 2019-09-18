using FluentAssertions;
using NUnit.Framework;
using SquareAccess.Services.Items;
using SquareAccessTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SquareAccessTests
{
	public class ItemsServiceTests : BaseTest
	{
		private ISquareItemsService _itemsService;
		private string _testSku = "testsku1";
		private string _testSku2 = "testsku2";

		[ SetUp ]
		public void Init()
		{
			this._itemsService = new SquareItemsService( this.Config, this.Credentials, new FakeLocationsService( this.LocationId.Id ) );
		}

		[ Test ]
		public void GetItemBySku()
		{
			var item = this._itemsService.GetItemBySkuAsync( _testSku, CancellationToken.None ).Result;

			item.Should().NotBeNull();
			item.Sku.Should().Be( item.Sku );
		}

		[ Test ]
		public void GetNonExistentItemBySku()
		{
			var sku = Guid.NewGuid().ToString();

			var item = this._itemsService.GetItemBySkuAsync( sku, CancellationToken.None ).Result;

			item.Should().BeNull();
		}

		[ Test ]
		public void GetItemsCreatedAfterGivenDate()
		{
			var items = this._itemsService.GetChangedItemsAfterAsync( new DateTime( 2019, 09, 10 ), CancellationToken.None ).Result;

			items.Should().NotBeNullOrEmpty();
		}

		[ Test ]
		public async Task UpdateSkuQuantity()
		{
			int quantity = 30;

			await _itemsService.UpdateSkuQuantityAsync( this._testSku, quantity, CancellationToken.None ).ConfigureAwait( false );
			var item = this._itemsService.GetItemBySkuAsync( this._testSku, CancellationToken.None ).Result;

			item.Should().NotBeNull();
			item.Quantity.Should().Be( quantity );
		}

		[ Test ]
		public async Task UpdateSkuQuantityToZero()
		{
			int initialQuantity = 10;
			int quantity = 0;

			await _itemsService.UpdateSkuQuantityAsync( this._testSku, initialQuantity, CancellationToken.None ).ConfigureAwait( false );
			await _itemsService.UpdateSkuQuantityAsync( this._testSku, quantity, CancellationToken.None ).ConfigureAwait( false );
			var item = this._itemsService.GetItemBySkuAsync( this._testSku, CancellationToken.None ).Result;

			item.Should().NotBeNull();
			item.Quantity.Should().Be( quantity );
		}

		[ Test ]
		public async Task UpdateSkusQuantities()
		{
			var skuPrefix = "testsku";
			var totalSkus = 10;
			var request = new Dictionary< string, int >();

			for ( int i = 1; i <= totalSkus; i++ )
			{
				request.Add( skuPrefix + i.ToString(), i );
			}

			await this._itemsService.UpdateSkusQuantityAsync( request, CancellationToken.None ).ConfigureAwait( false );

			var items = await this._itemsService.GetItemsBySkusAsync( request.Select( i => i.Key ), CancellationToken.None ).ConfigureAwait( false );

			foreach( var item in items )
			{
				var initialQuantity = request[ item.Sku ];
				item.Quantity.Should().Be( initialQuantity );
			}
		}
	}
}