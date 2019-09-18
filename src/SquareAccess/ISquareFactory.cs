using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareAuthenticationService CreateAuthenticationService();
		ISquareOrdersService CreateOrdersService( SquareMerchantCredentials credentials, Throttler throttler );
		ISquareItemsService CreateItemsService( SquareMerchantCredentials credentials );
	}
}
