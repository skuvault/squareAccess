using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareAuthenticationService CreateAuthenticationService();
		ISquareOrdersService CreateOrdersService( SquareConfig config, Throttler throttler );
	}
}
