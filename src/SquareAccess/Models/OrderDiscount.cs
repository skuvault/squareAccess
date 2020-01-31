using System.Collections.Generic;
using Square.Connect.Model;
using System.Linq;
using SquareAccess.Shared;
using Money = NMoneys.Money;
using System;

namespace SquareAccess.Models
{
	public class SquareOrderDiscount
	{
		public Money? Amount { get; set; }
		public string Code { get; set; }
		public SquareDiscountTypeEnum Type { get; set; }
	}

	public static class OrderDiscountExtensions
	{
		public static IEnumerable< SquareOrderDiscount > ToSvDiscounts( this IEnumerable< OrderLineItemDiscount > orderLineItemDiscounts )
		{
			if ( orderLineItemDiscounts == null )
			{
				return new List< SquareOrderDiscount >();
			}
			return orderLineItemDiscounts.Select( d => new SquareOrderDiscount
			{
				Amount = ( d.Scope == "ORDER" ? d.AmountMoney : d.AppliedMoney )?.ToNMoney(),   //TODO GUARD-324 Test this logic with actual orders
				Code = d.Name,
				Type = GetDiscountType( d.Type )
			} );
		}

		private static SquareDiscountTypeEnum GetDiscountType( string squareDiscountType )
		{
			switch( squareDiscountType )
			{
				case "FIXED_PERCENTAGE":
					return SquareDiscountTypeEnum.Percentage;
				case "FIXED_AMOUNT":
					return SquareDiscountTypeEnum.FixedAmount;
				default:
					return SquareDiscountTypeEnum.Undefined;
			}
		}
	}

	public enum SquareDiscountTypeEnum
	{
		Undefined,
		FixedAmount,
		Percentage
	}
}
