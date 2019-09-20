using System.Threading;
using System.Threading.Tasks;
using SquareAccess.Models;
using SquareAccess.Services.Customers;
using SquareAccess.Shared;

namespace SquareAccessTests.Mocks
{
	public class FakeCustomersService : ISquareCustomersService
	{
		public void Dispose()
		{
		}

		public Task< SquareCustomer > GetCustomerByIdAsync( string customerId, CancellationToken token, Mark mark )
		{
			var customer = new SquareCustomer
			{
				FirstName = "bud"
			};
			return Task.FromResult( customer );
		}
	}
}
