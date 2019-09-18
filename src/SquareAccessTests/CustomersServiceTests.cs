using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SquareAccess.Services.Customers;
using SquareAccess.Shared;

namespace SquareAccessTests
{
	public class CustomersServiceTests : BaseTest
	{
		private ISquareCustomersService _customersService;

		[ SetUp ]
		public void Init()
		{
			this._customersService = new SquareCustomersService( this.Config );
		}

		[ Test ]
		public void GetCustomerById()
		{
			const string customerId = "P3ARS3ZGCS5FQ0CXB2AEG7SDF0";

			var customer = _customersService.GetCustomerByIdAsync( customerId, CancellationToken.None, Mark.CreateNew() ).Result;

			customer.Should().NotBeNull();
		}
	}
}
