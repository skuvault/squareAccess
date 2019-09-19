using System;
using System.Threading;
using System.Threading.Tasks;
using Square.Connect.Model;
using SquareAccess.Shared;

namespace SquareAccess.Services.Locations
{
	public interface ISquareLocationsService : IDisposable
	{
		Task< ListLocationsResponse > GetLocationsAsync( CancellationToken token, Mark mark );
	}
}
