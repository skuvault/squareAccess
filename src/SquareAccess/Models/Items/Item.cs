using Square.Connect.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SquareAccess.Models.Items
{
	public class SquareItem
	{
		public string Id { get; set; }
		public string VariationId { get; set; }
		public string Name { get; set; }
		public string CategoryId { get; set; }
		public string Description { get; set; }
		public string Sku { get; set; }
		public string UPC { get; set; }
		public decimal? Price { get; set; }
		public string PriceCurrency { get; set; }
		public decimal? Quantity { get; set; }
	}

	public static class ItemsExtensions
	{
		public const string ItemCatalogObjectType = "ITEM";
		public const string ItemVariationCatalogObjectType = "ITEM_VARIATION";

		public static SquareItem[] ToSvItems( this CatalogObject catalogObject )
		{
			if ( !( catalogObject.Type == ItemCatalogObjectType || catalogObject.Type == ItemVariationCatalogObjectType ) )
				return null;

			if ( catalogObject.Type == ItemCatalogObjectType )
			{
				var items = new List< SquareItem >();

				foreach( var variation in catalogObject.ItemData.Variations )
				{
					var variationItem = variation.ToSvItems().First();

					items.Add( new SquareItem()
					{
						Id = catalogObject.Id,
						VariationId = variationItem.Id,
						CategoryId = catalogObject.ItemData.CategoryId,
						Description = catalogObject.ItemData.Description,
						Sku = variationItem.Sku,
						UPC = variationItem.UPC,
						Price = variationItem.Price,
						PriceCurrency = variationItem.PriceCurrency
					} );
				}

				return items.ToArray();
			}
			
			return new SquareItem[] { catalogObject.ToSvItemVariation() };
		}

		public static SquareItem ToSvItemVariation( this CatalogObject catalogObject )
		{
			if ( catalogObject.Type != ItemVariationCatalogObjectType )
				return null;

			return new SquareItem
			{
				Id = catalogObject.ItemVariationData.ItemId,
				VariationId = catalogObject.Id,
				Sku = catalogObject.ItemVariationData.Sku,
				UPC = catalogObject.ItemVariationData.Upc,
				Price = catalogObject.ItemVariationData.PriceMoney?.Amount,
				PriceCurrency = catalogObject.ItemVariationData.PriceMoney?.Currency
			};
		}
	}
}
