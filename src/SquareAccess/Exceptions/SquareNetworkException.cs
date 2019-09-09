using System;

namespace SquareAccess.Exceptions
{
	public class SquareNetworkException : SquareException
	{
		public SquareNetworkException( string message, Exception exception ) : base( message, exception) { }
		public SquareNetworkException( string message ) : base( message ) { }
	}

	public class SquareUnAuthorizedException : SquareException
	{
		public SquareUnAuthorizedException( string message ) : base( message) { }
	}

	public class SquareRateLimitsExceeded : SquareNetworkException
	{
		public SquareRateLimitsExceeded( string message ) : base( message ) { }
	}
}
