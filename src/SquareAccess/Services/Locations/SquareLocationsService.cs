using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Shared;

namespace SquareAccess.Services.Locations
{
	public class SquareLocationsService : BaseService, ISquareLocationsService
	{
		private LocationsApi _locationsApi;

		public SquareLocationsService( SquareConfig config ) : base( config )
		{
			_locationsApi = new LocationsApi
			{
				Configuration = new Square.Connect.Client.Configuration
				{
					AccessToken = this.Config.AccessToken
				}
			};
		}

		/// <summary>
		/// Get all locations for the store
		/// </summary>
		/// <param name="token">Cancellation token for cancelling call to endpoint</param>
		/// <param name="mark">Mark for log tracing</param>
		/// <returns>Locations</returns>
		public async Task< List< Location > > GetLocationsAsync( CancellationToken token, Mark mark )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( SquareEndPoint.ListLocationsUrl, mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var locationsResponse = await base.ThrottleRequest( SquareEndPoint.ListLocationsUrl, mark, ( _ ) =>
			{
				return _locationsApi.ListLocationsAsync();
			}, token ).ConfigureAwait( false );

			List< Location > locations = null;
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

			return locations;
		}
	}
}
