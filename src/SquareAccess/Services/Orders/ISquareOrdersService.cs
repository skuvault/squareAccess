using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SquareAccess.Models;

namespace SquareAccess.Services.Orders
{
	public interface ISquareOrdersService : IDisposable
	{
		Task< IEnumerable< SquareOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, CancellationToken token );
	}
}
