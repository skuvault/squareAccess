using SquareAccess.Configuration;
using SquareAccess.Services.Orders;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareOrdersService CreateOrdersService( SquareConfig config );
	}
}
