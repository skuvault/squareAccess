using SquareAccess.Configuration;
using SquareAccess.Services.Orders;
using SquareAccess.Throttling;

namespace SquareAccess
{
    public class SquareFactory : ISquareFactory
    {
	    public ISquareOrdersService CreateOrdersService(SquareConfig config, Throttler throttler)
	    {
		    throw new System.NotImplementedException();
	    }
    }
}
