using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Model;
using SquareAccess.Shared;

namespace SquareAccess.Services.Locations
{
	public interface ISquareLocationsService
	{
		Task< List< Location > > GetLocationsAsync( CancellationToken token, Mark mark );
	}
}
