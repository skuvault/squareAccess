using Square.Connect.Model;
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
		public static SquareOrderLineItem ToSvOrderLineItem( this OrderLineItem orderLineItem, CatalogObject catalogObject )
		{
			return new SquareOrderLineItem
			{
				Quantity = orderLineItem.Quantity,
				UnitPrice =  orderLineItem.BasePriceMoney?.ToNMoney(),
				Sku = catalogObject?.ItemVariationData?.Sku
			};
		}
	}
}