using Square.Connect.Model;
using SquareAccess.Models.Items;
using SquareAccess.Shared;
using Money = NMoneys.Money;

namespace SquareAccess.Models
{
	public class SquareOrderLineItem
	{
		public string Quantity { get; set; }
		public Money? UnitPrice { get; set; }
		public string Sku { get; set; }
	}

	public static class OrderLineItemExtensions
	{
		public static SquareOrderLineItem ToSvOrderLineItem( this OrderLineItem orderLineItem, SquareItem item )
		{
			if( item == null )
				return null;

			return new SquareOrderLineItem
			{
				Quantity = orderLineItem.Quantity,
				UnitPrice =  orderLineItem.BasePriceMoney?.ToNMoney(),
				Sku = item?.Sku
			};
		}
	}
}