using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SquareAccess.Models;
using SquareAccess.Shared;

namespace SquareAccess.Services.Locations
{
	public interface ISquareLocationsService : IDisposable
	{
		Task< IEnumerable< SquareLocation > > GetLocationsAsync( CancellationToken token, Mark mark );
		Task< IEnumerable< SquareLocation > > GetActiveLocationsAsync( CancellationToken token, Mark mark );
	}
}
