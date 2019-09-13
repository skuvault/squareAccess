using System.Collections.Generic;
using FluentAssertions;
using NMoneys.Extensions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;
using SquareAccess.Shared;

namespace SquareAccessTests
{
	public class OrderMapperTests
	{
		[ Test ]
		public void ToSvOrder()
		{
			const string orderId = "alskdf";
			const string locationId = "asldfkj";
			var order = new Order( orderId, locationId )
			{
				TotalMoney = new Money( 31 ),
				State = "COMPLETED",
				UpdatedAt = "2019-02-03T05:07:51Z",
				LineItems = new List<OrderLineItem>
				{

				}
			};
			var customer = new SquareCustomer
			{
				
			};
			const string catalogObjectType = "asdfasdf";
			const string catalogObjectId = "alsdkfj23lkj";
			var catalogObjects = new List< CatalogObject >
			{
				new CatalogObject( catalogObjectType, catalogObjectId )
				{
					ItemVariationData = new CatalogItemVariation()
				}
			};

			var result = order.ToSvOrder( customer, catalogObjects );

			result.CheckoutStatus.Should().Be( order.State );

			//TODO GUARD-203 Finish populating objects and check mappings
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
			var catalogObject = new CatalogObject( "asdf", catalogObjectId )
			{
				ItemVariationData = new CatalogItemVariation( "asdfjl", "", sku )
			};

			var result = orderLineItem.ToSvOrderLineItem( catalogObject );

			result.Sku.Should().Be( sku );
			result.Quantity.Should().Be( quantity );
			result.UnitPrice.Value.Amount.Should().Be( orderLineItem.BasePriceMoney.ToNMoney().Amount );
		}
	}
}
