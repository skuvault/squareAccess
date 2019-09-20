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
	public class OrderMapperTests
	{
		[ Test ]
		public void ToSvOrder()
		{
			const string catalogObjectId = "alsdkfj23lkj";
			const string catalogObjectId2 = "safd23232";
			const string quantity = "23";
			const string quantity2 = "9";
			var order = new Order( "alskdf", "asldfkj" )
			{
				TotalMoney = new Money( 31 ),
				State = "COMPLETED",
				CreatedAt = "2019-05-03T05:07:51Z",
				UpdatedAt = "2019-02-03T05:07:51Z",
				LineItems = new List< OrderLineItem >
				{
					new OrderLineItem( CatalogObjectId: catalogObjectId, Quantity: quantity ),
					new OrderLineItem( CatalogObjectId: catalogObjectId2, Quantity: quantity2 )
				}
			};
			var customer = new SquareCustomer
			{
				FirstName = "Bob"
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

			var result = order.ToSvOrder( customer, items );

			result.OrderId.Should().Be( order.Id );
			result.OrderTotal.Should().Be( order.TotalMoney.ToNMoney() );
			result.CheckoutStatus.Should().Be( order.State );
			result.CreateDateUtc.Should().Be( order.CreatedAt.FromRFC3339ToUtc() );
			result.UpdateDateUtc.Should().Be( order.UpdatedAt.FromRFC3339ToUtc() );
			result.LineItems.Count().Should().Be( items.Count ); 
			result.Customer.FirstName.Should().Be( customer.FirstName );
		}

		[ Test ]
		public void ToSvOrderLineItem()
		{
			const string quantity = "32";
			long? amount = 21;
			var unitPrice = new Money( amount );
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

			var result = orderLineItem.ToSvOrderLineItem( item );

			result.Sku.Should().Be( sku );
			result.Quantity.Should().Be( quantity );
			result.UnitPrice.Should().Be( orderLineItem.BasePriceMoney.ToNMoney() );
		}
	}
}
