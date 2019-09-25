using FluentAssertions;
using NUnit.Framework;
using Square.Connect.Model;
using SquareAccess.Models;

namespace SquareAccessTests
{
	public class CustomerMapperTests
	{
		[ Test ]
		public void ToSvCustomer()
		{
			var address = new Address
			{
				Locality = "Chicago",
				Country = "US",
				FirstName = "Bubba",
				LastName = "Gump",
				AddressLine1 = "123 Some St",
				AddressLine2 = "Unit 1",
				PostalCode = "1232",
				AdministrativeDistrictLevel1 = "Illinois"
			};
			var customer = new Customer( Id: "asldkfj", CreatedAt: "", UpdatedAt: "" )
			{
				Address = address,
				CompanyName = "ACME Inc",
				EmailAddress = "asdlkf@sdlk.oo",
				PhoneNumber = "122-123-1232"
			};

			var result = customer.ToSvCustomer();

			result.Country.Should().Be( address.Country );
			result.City.Should().Be( address.Locality );
			result.FirstName.Should().Be( address.FirstName );
			result.LastName.Should().Be( address.LastName );
			result.AddressLine1.Should().Be( address.AddressLine1 );
			result.AddressLine2.Should().Be( address.AddressLine2 );
			result.Postal.Should().Be( address.PostalCode );
			result.Region.Should().Be( address.AdministrativeDistrictLevel1 );
			result.Company.Should().Be( customer.CompanyName );
			result.Email.Should().Be( customer.EmailAddress );
			result.Phone.Should().Be( customer.PhoneNumber );
		}
	}
}