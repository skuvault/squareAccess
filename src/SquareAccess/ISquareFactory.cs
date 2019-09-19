using SquareAccess.Configuration;
using SquareAccess.Services.Authentication;
using SquareAccess.Services.Items;
using SquareAccess.Services.Orders;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareAuthenticationService CreateAuthenticationService();
		ISquareOrdersService CreateOrdersService( SquareConfig config );
		ISquareItemsService CreateItemsService();
	}
}
