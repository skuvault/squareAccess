using Square.Connect.Model;

namespace SquareAccess.Models
{
	public class SquareLocation
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public static class LocationExtensions
	{
		public static SquareLocation ToSvLocation( this Location location )
		{
			return new SquareLocation
			{
				Id = location.Id,
				Name = location.Name
			};
		}
	}
}
