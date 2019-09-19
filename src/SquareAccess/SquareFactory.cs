using CuttingEdge.Conditions;
using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Customers;
using SquareAccess.Services.Locations;
using SquareAccess.Services.Orders;

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

		public ISquareOrdersService CreateOrdersService( SquareConfig config )
		{
			var locationsService = new SquareLocationsService( this._config );
			return new SquareOrdersService( this._config, locationsService, new SquareCustomersService( this._config ), new SquareItemsService( this._config, locationsService) );
		}

		public ISquareItemsService CreateItemsService()
		{
			return new SquareItemsService( this._config, new SquareLocationsService( this._config ) );
		}
	}
}
