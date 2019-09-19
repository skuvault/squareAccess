using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
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
		public async Task< ListLocationsResponse > GetLocationsAsync( CancellationToken token, Mark mark )
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

							var response = await _locationsApi.ListLocationsAsync();

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
	}
}
