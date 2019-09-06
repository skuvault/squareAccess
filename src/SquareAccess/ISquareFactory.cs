using SquareAccess.Configuration;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
	public interface ISquareFactory
	{
		ISquareOrdersService CreateOrdersService( SquareConfig config, Throttler throttler );
	}
}
