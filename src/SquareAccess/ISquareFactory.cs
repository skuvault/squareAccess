using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Locations;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareAuthenticationService CreateAuthenticationService();
		ISquareLocationsService CreateLocationsService( SquareMerchantCredentials credentials );
		ISquareOrdersService CreateOrdersService( SquareMerchantCredentials credentials, Throttler throttler );
		ISquareItemsService CreateItemsService( SquareMerchantCredentials credentials );
	}
}