using System;
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
using SquareAccess.Services.Locations;
using SquareAccess.Shared;

namespace SquareAccess.Services.Orders
{
	public sealed class SquareOrdersService : AuthorizedBaseService, ISquareOrdersService
	{
		private ISquareLocationsService _locationsService;
		private OrdersApi _ordersApi;

		public SquareOrdersService( SquareConfig config, SquareMerchantCredentials credentials, ISquareLocationsService locationsService ) : base( config, credentials )
		{
			_locationsService = locationsService;
			_ordersApi = new OrdersApi
			{
				Configuration = new Square.Connect.Client.Configuration
				{
					AccessToken = this.Credentials.AccessToken
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

				var locations = await _locationsService.GetLocationsAsync( token, mark );

				if( locations == null || !locations.Locations.Any() )
				{
					var methodCallInfo = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() );
					var squareException  = new SquareException( string.Format( "{0}. No locations found", methodCallInfo ) );
					SquareLogger.LogTraceException( squareException );
					throw squareException;
				} 

				var errors = locations.Errors;
				if ( errors != null && errors.Any() )
				{
					var methodCallInfo = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
					var squareException  = new SquareException( string.Format( "{0}. Get locations returned errors", methodCallInfo ) );
					SquareLogger.LogTraceException( squareException );
					throw squareException;
				}

				SquareLogger.LogTrace( this.CreateMethodCallInfo( "", mark, payload: locations.Locations.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );

				response = await CollectOrdersFromAllPagesAsync( startDateUtc, endDateUtc, locations.Locations, 
					( requestBody ) => SearchOrdersAsync( requestBody, token, mark ), this.Config.OrdersPageSize );

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

		public static async Task< IEnumerable< SquareOrder > > CollectOrdersFromAllPagesAsync( DateTime startDateUtc, DateTime endDateUtc, List< Location > locations, Func< SearchOrdersRequest, Task < SearchOrdersResponse > > searchOrdersMethod, int ordersPerPage )
		{
			var orders = new List< SquareOrder >();
			var cursor = "";
			SearchOrdersRequest requestBody;
			SearchOrdersResponse ordersInPage;

			do
			{
				requestBody = CreateSearchOrdersBody( startDateUtc, endDateUtc, locations, cursor, ordersPerPage );
				ordersInPage = await searchOrdersMethod( requestBody );
				if( ordersInPage?.Orders != null ) 
				{ 
					orders.AddRange( ordersInPage.Orders.Select( o => o.ToSvOrder() ) );
					cursor = ordersInPage.Cursor;
				} else
				{
					cursor = "";
				}
			} while( !string.IsNullOrWhiteSpace( cursor ) );

			return orders;
		}

		private async Task< SearchOrdersResponse > SearchOrdersAsync( SearchOrdersRequest requestBody, CancellationToken token, Mark mark )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var responseContent = await Throttler.ExecuteAsync( () =>
			{
				return new Throttling.ActionPolicy( Config.NetworkOptions.RetryAttempts, Config.NetworkOptions.DelayBetweenFailedRequestsInSec, Config.NetworkOptions.DelayFailRequestRate )
					.ExecuteAsync( async () =>
					{
						using( var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource( token ) )
						{
							SquareLogger.LogStarted( this.CreateMethodCallInfo( "", mark, additionalInfo : this.AdditionalLogInfo() ) );
							linkedTokenSource.CancelAfter( Config.NetworkOptions.RequestTimeoutMs );

							var response = await _ordersApi.SearchOrdersAsync( requestBody );

							var errors = response.Errors;
							if ( errors != null && errors.Any() )
							{
								var methodCallInfo = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
								var squareException = new SquareException( string.Format( "{0}. Search orders returned errors", methodCallInfo ) );
								SquareLogger.LogTraceException( squareException );
								throw squareException;
							}

							//TODO GUARD-203 Should we log the return value only as trace?
							SquareLogger.LogEnd( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), methodResult: response.ToJson() ) );

							return response;
						}
					}, 
					( timeSpan, retryCount ) =>
					{
						string retryDetails = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() );
						SquareLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					},
					() => CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ),
					SquareLogger.LogTraceException );
			} ).ConfigureAwait( false );

			return responseContent;
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
								//TODO Do we need them all?
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
