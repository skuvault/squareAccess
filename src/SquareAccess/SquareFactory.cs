using CuttingEdge.Conditions;
using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
    public class SquareFactory : ISquareFactory
    {
		private SquareConfig _config;

		public SquareFactory( string applicationId, string applicationSecret )
		{
			Condition.Requires( applicationId, "applicationId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( applicationSecret, "applicationSecret" ).IsNotNullOrWhiteSpace();

			_config = new SquareConfig( applicationId, applicationSecret );
		}

		public ISquareAuthenticationService CreateAuthenticationService()
		{
			return new SquareAuthenticationService( this._config );
		}

		public ISquareOrdersService CreateOrdersService( SquareConfig config, Throttler throttler )
		{
			throw new System.NotImplementedException();
		}
	}
}
