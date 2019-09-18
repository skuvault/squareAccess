using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Api;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Models;
using SquareAccess.Shared;

namespace SquareAccess.Services.Customers
{
	public class SquareCustomersService : BaseService, ISquareCustomersService
	{
		private CustomersApi _customersApi;

		public SquareCustomersService( SquareConfig config ) : base( config )
		{
			_customersApi = new CustomersApi
			{
				Configuration = this.SquareConnectConfiguration
			};
		}

		public async Task< SquareCustomer > GetCustomerByIdAsync( string customerId, CancellationToken token, Mark mark )
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

							var response = await _customersApi.RetrieveCustomerAsync( customerId ).ConfigureAwait( false );

							var errors = response.Errors;
							if ( errors != null && errors.Any() )
							{
								var methodCallInfo = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
								var squareException = new SquareException( string.Format( "{0}. Get customer returned errors", methodCallInfo ) );
								SquareLogger.LogTraceException( squareException );
								throw squareException;
							}

							SquareLogger.LogEnd( this.CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo(), methodResult: response.ToJson() ) );

							return response.Customer;
						}
					}, 
					( timeSpan, retryCount ) =>
					{
						var retryDetails = CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() );
						SquareLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					},
					() => CreateMethodCallInfo( "", mark, additionalInfo: this.AdditionalLogInfo() ),
					SquareLogger.LogTraceException );
			} ).ConfigureAwait( false );

			return responseContent.ToSvCustomer();
		}
	}
}
