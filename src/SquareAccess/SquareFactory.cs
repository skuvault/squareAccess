using CuttingEdge.Conditions;
using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Customers;
using SquareAccess.Services.Locations;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
    public class SquareFactory : ISquareFactory
    {
		private SquareConfig _config;

		public SquareFactory( string applicationId, string applicationSecret, string accessToken )
		{
			Condition.Requires( applicationId, "applicationId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( applicationSecret, "applicationSecret" ).IsNotNullOrWhiteSpace();
			Condition.Requires( accessToken, "accessToken" ).IsNotNullOrWhiteSpace();

			_config = new SquareConfig( applicationId, applicationSecret, accessToken );
		}

		public ISquareAuthenticationService CreateAuthenticationService()
		{
			return new SquareAuthenticationService( this._config );
		}

		public ISquareOrdersService CreateOrdersService( SquareConfig config, Throttler throttler )
		{
			return new SquareOrdersService( this._config, new SquareLocationsService( this._config ), new SquareCustomersService( this._config ) );
		}

		public ISquareItemsService CreateItemsService()
		{
			return new SquareItemsService( this._config, new SquareLocationsService( this._config ) );
		}
	}
}
