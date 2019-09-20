using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
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
			Condition.Requires( customerId, "customerId" ).IsNotNullOrWhiteSpace();

			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( SquareEndPoint.RetrieveCustomerByIdUrl, mark, additionalInfo: this.AdditionalLogInfo() );
				var squareException = new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}

			var response = await base.ThrottleRequest( SquareEndPoint.RetrieveCustomerByIdUrl, mark, ( _ ) =>
			{
				return _customersApi.RetrieveCustomerAsync( customerId );
			}, token ).ConfigureAwait( false );

			var errors = response.Errors;
			if ( errors != null && errors.Any() )
			{
				var methodCallInfo = CreateMethodCallInfo( SquareEndPoint.RetrieveCustomerByIdUrl, mark, additionalInfo: this.AdditionalLogInfo(), errors: errors.ToJson() );
				var squareException = new SquareException( string.Format( "{0}. Get customer returned errors", methodCallInfo ) );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}


			return response.Customer.ToSvCustomer();
		}
	}
}
