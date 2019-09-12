using System;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Api;
using Square.Connect.Model;
using SquareAccess.Configuration;
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
		public async Task<ListLocationsResponse> GetLocationsAsync( CancellationToken token, Mark mark )
		{
			ListLocationsResponse response = null;

			try
			{
				SquareLogger.LogStarted( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ) );

				response = await _locationsApi.ListLocationsAsync();

				SquareLogger.LogEnd( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), methodResult: response.ToJson() ) );
			}
			catch ( Exception ex )
			{
				SquareLogger.LogTraceException( ex );
			}

			return response;
		}
	}
}
