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
		Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token, string locationId = null );
		Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token, string locationId = null );
		Task UpdateItemsQuantityAsync( IEnumerable< SquareItem> items, CancellationToken cancellationToken, string locationId = null );
	}
}
