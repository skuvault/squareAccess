using System;
using Square.Connect.Model;

namespace SquareAccess.Models
{
	public class SquareOrder
	{
		public string Id { get; set; }
		public Money TotalMoney { get; set; }
		public string State { get; set; }	//TODO Lookup?
		public DateTime UpdatedAt { get; set; }
	}

	public static class OrderExtensions
	{
		//TODO GUARD-203 Add Order properties
		public static SquareOrder ToSvOrder( this Order order )
		{
			return new SquareOrder
			{
				Id = order.Id
			};
		}
	}
}
