using System.Collections.Generic;
using System.Linq;
using Square.Connect.Model;

namespace SquareAccess.Models
{
	public class SquareOrderRecipient
	{
		public string City { get; set; }
		public string CountryCode { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string PostalCode { get; set; }
		public string Region { get; set; }
		public string ShippingClass { get; set; }
	}

	public static class OrderRecipientExtensions
	{
		public static SquareOrderRecipient ToSvRecipient( this IEnumerable< OrderFulfillment > fulfillments )
		{
			var fulfillment = fulfillments.FirstOrDefault();
			if( fulfillment == null )
			{
				return new SquareOrderRecipient();
			}

			var recipient = fulfillment.ShipmentDetails?.Recipient;
			if( recipient == null )
			{
				return new SquareOrderRecipient();
			}

			var svRecipient = new SquareOrderRecipient
			{
				Name = recipient.DisplayName,
				Email = recipient.EmailAddress,
				Phone = recipient.PhoneNumber,
				ShippingClass = fulfillment.ShipmentDetails.ShippingType
			};

			var address = recipient.Address;
			if( address != null ) 
			{ 
				svRecipient.City = address.Locality;
				svRecipient.CountryCode = address.Country;
				svRecipient.AddressLine1 = address.AddressLine1;
				svRecipient.AddressLine2 = address.AddressLine2;
				svRecipient.PostalCode = address.PostalCode;
				svRecipient.Region = address.AdministrativeDistrictLevel1;
			}
			return svRecipient;
		}
	}
}
