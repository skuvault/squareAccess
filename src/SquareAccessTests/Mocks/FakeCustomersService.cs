using System.Threading;
using System.Threading.Tasks;
using SquareAccess.Models;
using SquareAccess.Services.Customers;
using SquareAccess.Shared;

namespace SquareAccessTests.Mocks
{
	public class FakeCustomersService : ISquareCustomersService
	{
		public Task<SquareCustomer> GetCustomerByIdAsync(string customerId, CancellationToken token, Mark mark)
		{
			throw new System.NotImplementedException();
		}
	}
}
