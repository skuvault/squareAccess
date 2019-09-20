using Square.Connect.Model;

namespace SquareAccess.Models
{
	public class SquareLocation
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public bool Active { get; set; }
	}

	public static class LocationExtensions
	{
		public static string ActiveLocationStatus = "ACTIVE";

		public static SquareLocation ToSvLocation( this Location location )
		{
			return new SquareLocation
			{
				Id = location.Id,
				Name = location.Name,
				Active = location.Status == ActiveLocationStatus
			};
		}
	}
}