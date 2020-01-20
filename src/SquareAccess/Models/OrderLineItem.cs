using Square.Connect.Model;
using SquareAccess.Models.Items;
using SquareAccess.Shared;
using System.Collections.Generic;
using System.Linq;
using Money = NMoneys.Money;

namespace SquareAccess.Models
{
	public class SquareOrderLineItem
	{
		public string Quantity { get; set; }
		public Money? UnitPrice { get; set; }
		public string Sku { get; set; }
		public Money? TotalTax { get; set; }
		public List< SquareOrderDiscount > Discounts { get; set; }
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
				Sku = item?.Sku,
				Discounts = orderLineItem.Discounts?.ToSvDiscounts().ToList(),
				TotalTax = orderLineItem.TotalTaxMoney?.ToNMoney()
			};
		}
	}
}