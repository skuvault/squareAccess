using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;

namespace SquareAccessTests
{
	public class OrderRecipientMapperTests
	{
		[ Test ]
		public void ToSvRecipient_WhenNullRecipient_ReturnsEmpty()
		{
			var fulfillment = new OrderFulfillment
			{
				ShipmentDetails = new OrderFulfillmentShipmentDetails
				{
					Recipient = null
				}
			};

			var recipient = new List< OrderFulfillment >{ fulfillment }.ToSvRecipient();

			recipient.Should().BeEquivalentTo( new SquareOrderRecipient() );
		}

		[ Test ]
		public void ToSvRecipient_WhenNullAddress_ReturnsWithNoAddress()
		{
			const string name = "Bubba Gump";
			var fulfillment = new OrderFulfillment
			{
				ShipmentDetails = new OrderFulfillmentShipmentDetails
				{
					Recipient = new OrderFulfillmentRecipient
					{
						Address = null,
						DisplayName = name
					}
				}
			};

			var recipient = new List< OrderFulfillment >{ fulfillment }.ToSvRecipient();

			recipient.AddressLine1.Should().BeNullOrEmpty();
			recipient.Name.Should().Be( name );
		}

		[ Test ]
		public void ToSvRecipient_MapsFields()
		{
			const string shippingType = "donkey";
			const string name = "Bob";
			const string email = "asdf@ladsfk.com";
			const string phone = "123-123-1231";
			const string city = "Mayberry";
			const string countryCode = "US";
			const string addrLine1 = "123 Some St";
			const string addrLine2 = "Apt 1";
			const string postalCode = "12345";
			const string state = "AZ";
			var fulfillment = new OrderFulfillment
			{
				ShipmentDetails = new OrderFulfillmentShipmentDetails
				{
					ShippingType = shippingType,
					Recipient = new OrderFulfillmentRecipient
					{
						DisplayName = name,
						EmailAddress = email,
						PhoneNumber = phone,
						Address = new Address
						{
							Locality = city,
							Country = countryCode,
							AddressLine1 = addrLine1,
							AddressLine2 = addrLine2,
							PostalCode = postalCode,
							AdministrativeDistrictLevel1 = state
						}
					}
				}
			};

			var recipient = new List< OrderFulfillment >{ fulfillment }.ToSvRecipient();

			recipient.AddressLine1.Should().Be( addrLine1 );
			recipient.AddressLine2.Should().Be( addrLine2 );
			recipient.City.Should().Be( city );
			recipient.CountryCode.Should().Be( countryCode );
			recipient.Name.Should().Be( name );
			recipient.PostalCode.Should().Be( postalCode );
			recipient.Region.Should().Be( state );
			recipient.ShippingClass.Should().Be( shippingType );
			recipient.Email.Should().Be( email );
			recipient.Phone.Should().Be( phone );
		}
	}
}
