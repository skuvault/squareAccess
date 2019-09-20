using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Models;
using SquareAccess.Shared;

namespace SquareAccess.Services.Locations
{
	public class SquareLocationsService : AuthorizedBaseService, ISquareLocationsService
	{
		private LocationsApi _locationsApi;

		public SquareLocationsService( SquareConfig config, SquareMerchantCredentials credentials ) : base( config, credentials )
		{
			_locationsApi = new LocationsApi
			{
				Configuration = new Square.Connect.Client.Configuration
				{
					AccessToken = this.Credentials.AccessToken
				}
			};
		}

		/// <summary>
		/// Get all locations for the store
		/// </summary>
		/// <param name="token">Cancellation token for cancelling call to endpoint</param>
		/// <param name="mark">Mark for log tracing</param>
		/// <returns>Locations</returns>
		public async Task< IEnumerable< SquareLocation > > GetLocationsAsync( CancellationToken token, Mark mark )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( SquareEndPoint.ListLocationsUrl, mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var locationsResponse = await base.ThrottleRequest( SquareEndPoint.ListLocationsUrl, string.Empty, mark, cancellationToken =>
			{
				return _locationsApi.ListLocationsAsync();
			}, token ).ConfigureAwait( false );

			List< Location > locations = new List< Location >();
			if( locationsResponse == null || !( locations = locationsResponse.Locations ).Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.ListLocationsUrl, mark,
					additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. No locations found", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var errors = locationsResponse.Errors;
			if( errors != null && errors.Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.ListLocationsUrl, mark,
					additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
				var squareException =
					new SquareException( string.Format( "{0}. Get locations returned errors", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			return locations.Select( l => l.ToSvLocation() );
		}

		public async Task< IEnumerable< SquareLocation > > GetActiveLocationsAsync( CancellationToken token, Mark mark )
		{
			var locations = await this.GetLocationsAsync( token, mark ).ConfigureAwait( false );

			return locations.Where( l => l.Active );
		}
	}
}
