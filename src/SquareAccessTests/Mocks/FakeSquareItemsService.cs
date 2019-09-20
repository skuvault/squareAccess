using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SquareAccess.Models;
using SquareAccess.Models.Items;
using SquareAccess.Services.Items;
using SquareAccess.Shared;

namespace SquareAccessTests.Mocks
{
	public class FakeSquareItemsService : ISquareItemsService
	{
		public Task< SquareItem > GetItemBySkuAsync( string sku, CancellationToken token )
		{
			throw new NotImplementedException();
		}

		public Task< IEnumerable< SquareItem > > GetItemsBySkusAsync( IEnumerable< string > skus, CancellationToken cancellationToken )
		{
			throw new NotImplementedException();
		}

		public Task< IEnumerable< SquareItem > > GetChangedItemsAfterAsync( DateTime date, CancellationToken cancellationToken )
		{
			throw new NotImplementedException();
		}

		public Task< IEnumerable< SquareItem > > GetCatalogObjectsByIdsAsync( IEnumerable< string > catalogObjectsIds, CancellationToken cancellationToken, Mark mark )
		{
			var squareItems = new List< SquareItem >
			{
				new SquareItem
				{
					Id = "asdlfk"
				}
			};
			return Task.FromResult( squareItems.AsEnumerable() );
		}

		public Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token, LocationId locationId = null )
		{
			throw new NotImplementedException();
		}

		public Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token, LocationId locationId = null )
		{
			throw new NotImplementedException();
		}

		public Task UpdateItemsQuantityAsync( IEnumerable< SquareItem > items, CancellationToken cancellationToken, LocationId locationId = null )
		{
			throw new NotImplementedException();
		}
	}
}
