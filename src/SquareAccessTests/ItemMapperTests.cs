using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models.Items;

namespace SquareAccessTests
{
	public class ItemMapperTests
	{
		[ Test ]
		public void ToSvItemVariation()
		{
			const string id = "asdfsdf";
			const string variationId = "sladkfja";
			const string sku = "testsku";
			const string upc = "SLFDKDJ";
			long? price = 12;
			const string currency = "USD";
			var catalogObject = new CatalogObject( ItemsExtensions.ItemVariationCatalogObjectType, variationId )
			{
				ItemVariationData = new CatalogItemVariation( id )
				{
					Sku = sku,
					Upc = upc,
					PriceMoney = new Money( price, currency )
				}
			};

			var result = catalogObject.ToSvItemVariation();

			result.Id.Should().Be( id );
			result.VariationId.Should().Be( variationId );
			result.Sku.Should().Be( sku );
			result.UPC.Should().Be( upc );
			result.Price.Should().Be( price );
			result.PriceCurrency.Should().Be( currency );
		}
	}
}
