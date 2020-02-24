using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;
using SquareAccess.Models.Items;
using SquareAccess.Shared;

namespace SquareAccessTests
{
	[ TestFixture ]
	public class OrderMapperTests
	{
		[ Test ]
		public void ToSvOrder()
		{
			const string catalogObjectId = "alsdkfj23lkj";
			const string catalogObjectId2 = "safd23232";
			const string quantity = "23";
			const string quantity2 = "9";
			const string name = "alskja";
			Money totalTax = new Money( 7, "DOP" );
			var order = new Order( "alskdf", "asldfkj" )
			{
				TotalMoney = new Money( 31, "USD" ),
				State = "COMPLETED",
				CreatedAt = "2019-05-03T05:07:51Z",
				UpdatedAt = "2019-02-03T05:07:51Z",
				LineItems = new List< OrderLineItem >
				{
					new OrderLineItem( CatalogObjectId: catalogObjectId, Quantity: quantity ),
					new OrderLineItem( CatalogObjectId: catalogObjectId2, Quantity: quantity2 )
				},
				Fulfillments = new List< OrderFulfillment >
				{
					new OrderFulfillment
					{
						ShipmentDetails = new OrderFulfillmentShipmentDetails
						{
							Recipient = new OrderFulfillmentRecipient
							{
								DisplayName = name
							}
						}
					}
				},
				Discounts = new List< OrderLineItemDiscount >
				{
					new OrderLineItemDiscount()
				},
				TotalTaxMoney = totalTax,
				Tenders = new List< Tender >
				{
					new Tender( Id: "12345xyz", Type: "CASH" )
				}
			};
			var items = new List< SquareItem >
			{
				new SquareItem()
				{
					VariationId = catalogObjectId
				},
				new SquareItem()
				{
					VariationId = catalogObjectId2
				}
			};

			var result = order.ToSvOrder( items );

			result.OrderId.Should().Be( order.Id );
			result.OrderTotal.Should().Be( order.TotalMoney.ToNMoney() );
			result.ReceiptId.Should().Be( order.Tenders.First().Id );
			result.CheckoutStatus.Should().Be( order.State );
			result.CreateDateUtc.Should().Be( order.CreatedAt.FromRFC3339ToUtc() );
			result.UpdateDateUtc.Should().Be( order.UpdatedAt.FromRFC3339ToUtc() );
			result.LineItems.Count().Should().Be( items.Count ); 
			result.Recipient.Name.Should().Be( name );
			result.Discounts.Count().Should().Be( order.Discounts.Count() );
			result.TotalTax.Should().Be( totalTax.ToNMoney() );
		}

		[ Test ]
		public void ToSvOrderLineItem()
		{
			const string quantity = "32";
			long? amount = 21;
			var unitPrice = new Money( amount, "USD" );
			var orderLineItem = new OrderLineItem( null, null, Quantity: quantity )
			{
				BasePriceMoney = unitPrice
			};
			const string sku = "testSku3";
			const string catalogObjectId = "asdfsdf";
			var item = new SquareItem
			{
				VariationId = catalogObjectId,
				Sku = sku
			};
			Money totalTax = new Money( 6, "SOS" );
			orderLineItem.Discounts = new List<OrderLineItemDiscount>
			{
				new OrderLineItemDiscount(),
				new OrderLineItemDiscount()
			};
			orderLineItem.TotalTaxMoney = totalTax;

			var result = orderLineItem.ToSvOrderLineItem( item );

			result.Sku.Should().Be( sku );
			result.Quantity.Should().Be( quantity );
			result.UnitPrice.Should().Be( orderLineItem.BasePriceMoney.ToNMoney() );
			result.Discounts.Count().Should().Be( orderLineItem.Discounts.Count() );
			result.TotalTax.Should().Be( totalTax.ToNMoney() );
		}

		[ Test ]
		public void ToSvOrderLineItems()
		{
			const string catalogObjectId = "lasdkfasdfasdjlk";
			const string catalogObjectId2 = "as;lkdfj23r422";
			var quantity = "1";
			var basePrice = new Money( 123, "USD" );
			var orderLineItems = new List< OrderLineItem >
			{
				new OrderLineItem( Quantity: quantity, CatalogObjectId: catalogObjectId, BasePriceMoney: basePrice ),
				new OrderLineItem( Quantity: "2", CatalogObjectId: catalogObjectId2, BasePriceMoney: new Money( 334, "USD" ) )
			};
			const string sku = "testSku";
			var orderCatalogObjects = new List< SquareItem >
			{
				new SquareItem
				{
					VariationId = catalogObjectId,
					Sku = sku,
				}
			};

			var svOrderLineItems = orderLineItems.ToSvOrderLineItems( orderCatalogObjects ).ToList();

			svOrderLineItems.Should().HaveCount( 1 );
			svOrderLineItems[ 0 ].Quantity.Should().Be( quantity );
			svOrderLineItems[ 0 ].Sku.Should().Be( sku );
			svOrderLineItems[ 0 ].UnitPrice.Value.Should().Be( basePrice.ToNMoney() );
		}

		[ Test ]
		public void ToSvDiscounts()
		{
			var orderDiscount = new OrderLineItemDiscount
			{
				AmountMoney = new Money( 3, "USD" ),
				Scope = SquareDiscountScope.Order,
				Name = "Some order discount"
			};
			var itemDiscount = new OrderLineItemDiscount
			{
				AppliedMoney = new Money( 4, "GBP" ),
				Scope = SquareDiscountScope.LineItem,
				Name = "Some item discount"
			};
			var discounts = new List< OrderLineItemDiscount > 
			{
				orderDiscount,
				itemDiscount
			};

			var result = discounts.ToSvDiscounts();

			var resultOrderDiscount = result.First();
			var resultItemDiscount = result.Skip( 1 ).First();
			resultOrderDiscount.Amount.Value.Should().Be( orderDiscount.AmountMoney.ToNMoney() );
			resultOrderDiscount.Code.Should().Be( orderDiscount.Name );
			resultItemDiscount.Amount.Value.Should().Be( itemDiscount.AppliedMoney.ToNMoney() );
			resultItemDiscount.Code.Should().Be( itemDiscount.Name );
		}

		[ Test ]
		public void ToSvDiscounts_Types()
		{
			var fixedAmountDiscount = new OrderLineItemDiscount
			{
				Type = "FIXED_AMOUNT"
			};
			var percentageDiscount = new OrderLineItemDiscount
			{
				Type = "FIXED_PERCENTAGE"
			};
			var unknownDiscount = new OrderLineItemDiscount
			{
				Type = "MARY_HAD_A_LITTLE_LAMB"
			};
			var discounts = new List< OrderLineItemDiscount > 
			{
				fixedAmountDiscount,
				percentageDiscount,
				unknownDiscount
			};

			var result = discounts.ToSvDiscounts().ToArray();

			result[ 0 ].Type.Should().Be( SquareDiscountTypeEnum.FixedAmount );
			result[ 1 ].Type.Should().Be( SquareDiscountTypeEnum.Percentage );
			result[ 2 ].Type.Should().Be( SquareDiscountTypeEnum.Undefined );
		}
	}
}
