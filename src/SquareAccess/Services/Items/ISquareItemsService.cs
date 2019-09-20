using SquareAccess.Models;
using SquareAccess.Models.Items;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SquareAccess.Services.Items
{
	public interface ISquareItemsService
	{
		Task< SquareItem > GetItemBySkuAsync( string sku, CancellationToken token );
		Task< IEnumerable< SquareItem > > GetItemsBySkusAsync( IEnumerable< string > skus, CancellationToken cancellationToken );
		Task< IEnumerable< SquareItem > > GetChangedItemsAfterAsync( DateTime date, CancellationToken cancellationToken );
		Task< IEnumerable< SquareItem > > GetCatalogObjectsByIdsAsync( IEnumerable< string > catalogObjectsIds, CancellationToken cancellationToken, Shared.Mark mark );
		Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token, LocationId locationId = null );
		Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token, LocationId locationId = null );
		Task UpdateItemsQuantityAsync( IEnumerable< SquareItem> items, CancellationToken cancellationToken, LocationId locationId = null );
	}
}