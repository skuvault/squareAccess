﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Models;
using SquareAccess.Services.Customers;
using SquareAccess.Services.Locations;
using SquareAccess.Shared;

namespace SquareAccess.Services.Orders
{
	public sealed class SquareOrdersService : BaseService, ISquareOrdersService
	{
		private readonly ISquareLocationsService _locationsService;
		private readonly ISquareCustomersService _customersService;
		private readonly OrdersApi _ordersApi;

		public delegate Task< SquareOrdersBatch > GetOrdersWithRelatedDataAsyncDelegate( SearchOrdersRequest requestBody );

		public SquareOrdersService( SquareConfig config, ISquareLocationsService locationsService, ISquareCustomersService customersService ) : base( config )
		{
			Condition.Requires( locationsService, "locationsService" ).IsNotNull();

			_locationsService = locationsService;
			_customersService = customersService;
			_ordersApi = new OrdersApi
			{
				Configuration = new Square.Connect.Client.Configuration
				{
					AccessToken = this.Config.AccessToken
				}
			};
		}

		/// <summary>
		///	Returns orders created/modified between the start and end date
		/// </summary>
		/// <param name="startDateUtc"></param>
		/// <param name="endDateUtc"></param>
		/// <param name="token">Cancellation token for cancelling call to endpoint</param>
		/// <returns></returns>
		public async Task< IEnumerable< SquareOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, CancellationToken token )
		{
			//TODO GUARD-203 If we need to get orders for only the selected locations (on channel accounts page) then add locationNames List< string > parameter

			Condition.Requires( startDateUtc ).IsLessThan( endDateUtc );

			var mark = Mark.CreateNew();
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			IEnumerable< SquareOrder > response = null;

			try
			{
				SquareLogger.LogStarted( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ) );

				//TODO GUARD-203 If we need to get orders for only the selected locations (on channel accounts page) then add locationNames List< string > parameter
				var locations = await _locationsService.GetLocationsAsync( token, mark );

				SquareLogger.LogTrace( this.CreateMethodCallInfo( "", mark, payload: locations.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );

				response = await CollectOrdersFromAllPagesAsync( startDateUtc, endDateUtc, locations, 
					( requestBody ) => GetOrdersWithRelatedDataAsync( requestBody, token, mark ), this.Config.OrdersPageSize );

				SquareLogger.LogEnd( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ) );
			}
			catch ( Exception ex )
			{
				var squareException = new SquareException( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ), ex );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			return response;
		}

		public static async Task< IEnumerable< SquareOrder > > CollectOrdersFromAllPagesAsync( DateTime startDateUtc, DateTime endDateUtc, List< Location > locations, GetOrdersWithRelatedDataAsyncDelegate getOrdersWithRelatedDataMethod, int ordersPerPage )
		{
			var orders = new List< SquareOrder >();
			var cursor = "";
			SearchOrdersRequest requestBody;
			SquareOrdersBatch ordersInPage;

			do
			{
				requestBody = CreateSearchOrdersBody( startDateUtc, endDateUtc, locations, cursor, ordersPerPage );
				ordersInPage = ( await getOrdersWithRelatedDataMethod( requestBody ) );
				if( ordersInPage?.Orders != null ) 
				{ 
					orders.AddRange( ordersInPage.Orders );	
					cursor = ordersInPage.Cursor;
				} else
				{
					cursor = "";
				}
			} while( !string.IsNullOrWhiteSpace( cursor ) );

			return orders;
		}

		private async Task< SquareOrdersBatch > GetOrdersWithRelatedDataAsync( SearchOrdersRequest requestBody, CancellationToken token, Mark mark )
		{
			var result = await SearchOrdersAsync( requestBody, token, mark );

			if( result != null )
			{
				var orders = result.Orders; 
				var cursor = result.Cursor;

				var ordersWithRelatedData = new List< SquareOrder >();

				if ( orders != null && orders.Any() )
				{
					foreach ( var order in orders )
					{
						var customer = await _customersService.GetCustomerByIdAsync( order.CustomerId, token, mark );

						//TODO GUARD-203 For each OrderLineItem 
						//	Get CatalogObject by CatalogObject and pass into the line item mapper?
						//	Potentially, get them in batch for the entire order?
						IEnumerable<CatalogObject> catalogObjects = null; // GetCatalogObjectsByIdAsync( order.LineItems.Select( l => l.CatalogObjectId ));
						ordersWithRelatedData.Add( order.ToSvOrder( customer, catalogObjects ) );
					}

					return new SquareOrdersBatch
					{
						Orders = ordersWithRelatedData,
						Cursor = cursor
					};
				}
			}

			return new SquareOrdersBatch
			{
				Orders = new List< SquareOrder >(),
				Cursor = null
			};
		}

		private async Task< SearchOrdersResponse > SearchOrdersAsync( SearchOrdersRequest requestBody, CancellationToken token, Mark mark )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( SquareEndPoint.OrdersSearchUrl, mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var response = await base.ThrottleRequest( SquareEndPoint.SearchCatalogUrl, mark, ( _ ) =>
			{
				return  _ordersApi.SearchOrdersAsync( requestBody );
			}, token ).ConfigureAwait( false );

			var errors = response.Errors;
			if ( errors != null && errors.Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.OrdersSearchUrl, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
				var squareException = new SquareException( string.Format( "{0}. Search orders returned errors", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			return response;
		}

		public static SearchOrdersRequest CreateSearchOrdersBody( DateTime startDateUtc, DateTime endDateUtc, List< Location > locations, string cursor, int ordersPerPage )
		{
			var updatedAtStart = startDateUtc.FromUtcToRFC3339();
			var updatedAtEnd = endDateUtc.FromUtcToRFC3339();

			var body = new SearchOrdersRequest
			{
				ReturnEntries = false,
				Limit = ordersPerPage,
				LocationIds = locations.Select( l => l.Id ).ToList(),
				Query = new SearchOrdersQuery
				{
					Filter = new SearchOrdersFilter
					{
						DateTimeFilter = new SearchOrdersDateTimeFilter
						{
							UpdatedAt = new TimeRange
							{
								StartAt = updatedAtStart,
								EndAt = updatedAtEnd
							}
						},
						StateFilter = new SearchOrdersStateFilter( 
							new List< string >
							{
								"COMPLETED",
								"OPEN",
								"CANCELED"
							}
						)
					},
					Sort = new SearchOrdersSort("UPDATED_AT", "DESC")
				}
			};

			if( !string.IsNullOrWhiteSpace( cursor ))
			{
				body.Cursor = cursor;
			}

			return body;
		}
	}
}
