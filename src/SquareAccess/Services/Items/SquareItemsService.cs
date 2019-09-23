using CuttingEdge.Conditions;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Models.Items;
using SquareAccess.Services.Locations;
using SquareAccess.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SquareAccess.Services.Items
{
	public sealed class SquareItemsService : AuthorizedBaseService, ISquareItemsService
	{
		private CatalogApi _catalogApi;
		private InventoryApi _inventoryApi;
		private ISquareLocationsService _locationsService;
		private string _locationId;
		
		private const string InventoryChangeType = "PHYSICAL_COUNT";
		private const string InventoryItemState = "IN_STOCK";

		public SquareItemsService( SquareConfig config, SquareMerchantCredentials credentials, ISquareLocationsService locationsService ) : base( config, credentials )
		{
			Condition.Requires( locationsService, "locationsService" ).IsNotNull();

			var apiConfig = new Square.Connect.Client.Configuration {
				AccessToken = this.Credentials.AccessToken
			};

			this._catalogApi = new CatalogApi( apiConfig );
			this._inventoryApi = new InventoryApi( apiConfig );
			this._locationsService = locationsService;
		}

		/// <summary>
		///	Returns Square item variations which have specified skus
		/// </summary>
		/// <param name="skus">List of skus</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task< IEnumerable< SquareItem > > GetItemsBySkusAsync( IEnumerable< string > skus, CancellationToken cancellationToken )
		{
			var result = new List< SquareItem >();

			foreach( var sku in skus )
			{
				var item = await this.GetItemBySkuAsync( sku, cancellationToken ).ConfigureAwait( false );

				if ( item != null )
				{
					result.Add( item );
				}
			}

			return result;
		}

		/// <summary>
		///	Returns Square item variation with specified sku
		/// </summary>
		/// <param name="sku">Sku</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task< SquareItem > GetItemBySkuAsync( string sku, CancellationToken cancellationToken )
		{
			Condition.Requires( sku, "sku" ).IsNotNullOrWhiteSpace();

			var mark = Mark.CreateNew();

			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url: SquareEndPoint.SearchCatalogUrl, mark: mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var request = new SearchCatalogObjectsRequest
			{
				IncludeDeletedObjects = false,
				IncludeRelatedObjects = true,
				ObjectTypes = new string[] { ItemsExtensions.ItemVariationCatalogObjectType }.ToList(),
				Query = new CatalogQuery()
				{
					ExactQuery = new CatalogQueryExact( "sku", sku )
				}
			};

			var response = await base.ThrottleRequest( SquareEndPoint.SearchCatalogUrl, request.ToJson(), mark, ( token ) =>
			{
				return this._catalogApi.SearchCatalogObjectsAsync( request );
			}, cancellationToken ).ConfigureAwait( false );

			var errors = response.Errors;
			if ( errors != null && errors.Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.SearchCatalogUrl, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
				var squareException  = new SquareException( string.Format( "{0}. Search items catalog returned errors", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var catalogItem = response.Objects?.FirstOrDefault( o => o.ItemVariationData != null );

			if ( catalogItem != null )
			{
				 var item = catalogItem.ToSvItems().First();
				 var itemsWithQuantity = await this.FillItemsQuantities( new SquareItem[] { item }.ToList(), cancellationToken ).ConfigureAwait( false );
				 return itemsWithQuantity.FirstOrDefault();
			}

			return null;
		}

		/// <summary>
		///	Returns Square items which were updated or created after specified date
		/// </summary>
		/// <param name="date"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task< IEnumerable< SquareItem > > GetChangedItemsAfterAsync( DateTime date, CancellationToken cancellationToken )
		{
			var items = new List< SquareItem >();
			var mark = Mark.CreateNew();

			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url: SquareEndPoint.SearchCatalogUrl, mark: mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			string paginationCursor = null;

			do
			{
				var request = new SearchCatalogObjectsRequest
				{
					IncludeDeletedObjects = false,
					IncludeRelatedObjects = true,
					Cursor = paginationCursor,
					ObjectTypes = new string[] { ItemsExtensions.ItemCatalogObjectType }.ToList(),
					BeginTime = date.ToUniversalTime().FromUtcToRFC3339()
				};

				var response = await base.ThrottleRequest( SquareEndPoint.SearchCatalogUrl, request.ToJson(), mark, ( token ) =>
				{
					return this._catalogApi.SearchCatalogObjectsAsync( request );
				}, cancellationToken ).ConfigureAwait( false );

				var errors = response.Errors;
				if ( errors != null && errors.Any() )
				{
					var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.SearchCatalogUrl, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
					var squareException  = new SquareException( string.Format( "{0}. Search items catalog returned errors", methodCallInfo ) );
					SquareLogger.LogTraceException( squareException );
					throw squareException;
				}

				if ( response.Objects != null )
				{
					foreach( var obj in response.Objects )
					{
						items.AddRange( obj.ToSvItems() );
					}
				}

				paginationCursor = response.Cursor;
			}
			while( !string.IsNullOrWhiteSpace( paginationCursor ) );

			return items;
		}

		/// <summary>
		///	Returns Square items with the supplied catalogObjectIds
		/// </summary>
		/// <param name="catalogObjectsIds"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		public async Task< IEnumerable< SquareItem > > GetCatalogObjectsByIdsAsync( IEnumerable< string > catalogObjectsIds, CancellationToken cancellationToken, Mark mark )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url: SquareEndPoint.BatchRetrieveCatalogObjectsUrl, mark: mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			if( catalogObjectsIds == null || !catalogObjectsIds.Any() )
			{
				return null;
			}

			var requestBody = new BatchRetrieveCatalogObjectsRequest( catalogObjectsIds.ToList() );

			var response = await base.ThrottleRequest( SquareEndPoint.SearchCatalogUrl, requestBody.ToJson(), mark, ( token ) =>
			{
				return this._catalogApi.BatchRetrieveCatalogObjectsAsync( requestBody );
			}, cancellationToken ).ConfigureAwait( false );

			var errors = response.Errors;
			if ( errors != null && errors.Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.BatchRetrieveCatalogObjectsUrl, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
				var squareException  = new SquareException( string.Format( "{0}. Batch retrieve catalog objects errors", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			return response.Objects?.Select( c => c.ToSvItemVariation() );
		}

		/// <summary>
		///	Updates Square items variations quantity
		/// </summary>
		/// <param name="skusQuantities">Sku with quantity</param>
		/// <param name="location">Square location. If argument is not specified the first location will be used</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken cancellationToken, string locationId  = null )
		{
			if ( skusQuantities.Count == 0 )
				return;

			if ( string.IsNullOrWhiteSpace( locationId ) )
				locationId = GetDefaultLocationId();

			var items = await this.GetItemsBySkusAsync( skusQuantities.Select( s => s.Key ), cancellationToken ).ConfigureAwait( false );
			var request = new List< SquareItem >();

			foreach( var item in items )
			{
				if ( skusQuantities.ContainsKey( item.Sku.ToLower() ) )
				{
					item.Quantity = skusQuantities[ item.Sku.ToLower() ];
					request.Add( item );
				}
			}

			await this.UpdateItemsQuantityAsync( request, cancellationToken, locationId ).ConfigureAwait( false );
		}

		/// <summary>
		///	Update Square item variation quantity
		/// </summary>
		/// <param name="sku">Sku</param>
		/// <param name="quantity">Quantity</param>
		/// <param name="token"></param>
		/// <returns></returns>
		public Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token, string locationId )
		{
			return UpdateSkusQuantityAsync( new Dictionary< string, int >() { { sku, quantity } }, token, locationId );
		}

		/// <summary>
		///	Fills Square item quantity field using inventory api
		/// </summary>
		/// <param name="items"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task< IEnumerable< SquareItem > > FillItemsQuantities( IEnumerable< SquareItem > items, CancellationToken cancellationToken )
		{
			var mark = Mark.CreateNew();

			var request = new BatchRetrieveInventoryCountsRequest
			{
				CatalogObjectIds = items.Select( i => i.VariationId ).ToList(),
				LocationIds = new string[] { this.GetDefaultLocationId() }.ToList()
			};
			string paginationCursor = null;

			do
			{
				var response = await base.ThrottleRequest( SquareEndPoint.RetrieveInventoryCounts, request.ToJson(), mark, token => {
					return this._inventoryApi.BatchRetrieveInventoryCountsAsync( request );
				}, cancellationToken ).ConfigureAwait( false );

				var errors = response.Errors;
				if ( errors != null && errors.Any() )
				{
					var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.RetrieveInventoryCounts, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
					var squareException  = new SquareException( string.Format( "{0}. Search items catalog returned errors", methodCallInfo ) );
					SquareLogger.LogTraceException( squareException );
					throw squareException;
				}

				if ( response.Counts != null && response.Counts.Any() )
				{
					foreach( var inventoryItem in response.Counts )
					{
						var item = items.FirstOrDefault( i => i.VariationId == inventoryItem.CatalogObjectId );

						if ( item != null )
						{
							item.Quantity = decimal.Parse( inventoryItem.Quantity );
						}
					}
				}

				paginationCursor = response.Cursor;
			}
			while ( !string.IsNullOrWhiteSpace( paginationCursor ) );

			return items;
		}

		/// <summary>
		///	Updates items variations quantity
		/// </summary>
		/// <param name="items"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task UpdateItemsQuantityAsync( IEnumerable< SquareItem> items, CancellationToken cancellationToken, string locationId = null )
		{
			if ( items.Count() == 0 )
				return;

			var mark = Mark.CreateNew();

			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url: SquareEndPoint.BatchChangeInventory, mark: mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			if ( locationId == null )
				locationId = this.GetDefaultLocationId();

			var request = new BatchChangeInventoryRequest()
			{
				IdempotencyKey = mark.MarkValue, 
				Changes = items.Select( i => new InventoryChange() { 
									Type = InventoryChangeType, 
									PhysicalCount = new InventoryPhysicalCount( 
										CatalogObjectId: i.VariationId,  
										State: InventoryItemState,
										OccurredAt: DateTime.UtcNow.FromUtcToRFC3339(),
										LocationId: locationId,
										Quantity: i.Quantity.ToString() ) } ).ToList()
			};

			await base.ThrottleRequest( SquareEndPoint.BatchChangeInventory, request.ToJson(), mark, async token =>
			{
				var response = await this._inventoryApi.BatchChangeInventoryAsync( request ).ConfigureAwait( false );
				
				if ( response.Errors != null && response.Errors.Any() )
				{
					var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.BatchChangeInventory, mark, additionalInfo: this.AdditionalLogInfo(), errors: response.Errors.ToJson() );
					var squareException  = new SquareException( string.Format( "{0}. Search items catalog returned errors", methodCallInfo ) );
					SquareLogger.LogTraceException( squareException );
					throw squareException;
				}

				return response.Counts;
			}, cancellationToken ).ConfigureAwait( false );
		}

	
		/// <summary>
		///	Returns Square default location
		/// </summary>
		/// <returns></returns>
		private string GetDefaultLocationId()
		{
			if ( !string.IsNullOrWhiteSpace( this._locationId ) )
				return this._locationId;

			var locations = this._locationsService.GetActiveLocationsAsync( CancellationToken.None, null ).Result;

			if ( locations.Count() > 1 )
				throw new SquareException( "Can't use default location. Square account has more than one. Specify location" );

			this._locationId = locations.Select( l => l.Id ).FirstOrDefault();
			
			return this._locationId;
		}
	}
}
