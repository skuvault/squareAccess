using System;

namespace SquareAccess.Exceptions
{
	public class SquareException : Exception
	{
		public SquareException( string message, Exception exception ): base( message, exception ) { }
		public SquareException( string message ) : base( message ) { }
	}
}
