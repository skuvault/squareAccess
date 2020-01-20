using CuttingEdge.Conditions;
using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Locations;
using SquareAccess.Services.Orders;

namespace SquareAccess
{
    public class SquareFactory : ISquareFactory
    {
		private SquareConfig _config;

		public SquareFactory( string applicationId, string applicationSecret, bool isSandbox = false )
		{
			Condition.Requires( applicationId, "applicationId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( applicationSecret, "applicationSecret" ).IsNotNullOrWhiteSpace();

			_config = new SquareConfig( applicationId, applicationSecret, isSandbox );
		}

		public ISquareAuthenticationService CreateAuthenticationService()
		{
			return new SquareAuthenticationService( this._config );
		}

		public ISquareLocationsService CreateLocationsService( SquareMerchantCredentials credentials )
		{
			return new SquareLocationsService( this._config, credentials );
		}

		public ISquareOrdersService CreateOrdersService( SquareMerchantCredentials credentials )
		{
			var locationsService = new SquareLocationsService( this._config, credentials );
			var itemsService = new SquareItemsService( this._config, credentials, locationsService );

			return new SquareOrdersService( this._config, credentials, locationsService, itemsService );
		}

		public ISquareItemsService CreateItemsService( SquareMerchantCredentials credentials )
		{
			return new SquareItemsService( this._config, credentials, new SquareLocationsService( this._config, credentials ) );
		}
	}
}
