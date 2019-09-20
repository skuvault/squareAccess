using Square.Connect.Model;

namespace SquareAccess.Models
{
	public class SquareCustomer
	{
		public string City { get; set; }
		public string Country { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string Postal { get; set; }
		public string Region { get; set; }
		public string Company { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
	}

	public static class CustomerExtensions
	{
		public static SquareCustomer ToSvCustomer( this Customer customer )
		{
			if( customer.Address == null )
			{
				return null;
			}

			return new SquareCustomer
			{
				City = customer.Address.Locality,
				Country = customer.Address.Country,		
				FirstName = customer.Address.FirstName,	
				LastName = customer.Address.LastName,	
				AddressLine1 = customer.Address.AddressLine1,
				AddressLine2 = customer.Address.AddressLine2,
				Postal = customer.Address.PostalCode,
				Region = customer.Address.AdministrativeDistrictLevel1,
				Company = customer.CompanyName,
				Email = customer.EmailAddress,
				Phone = customer.PhoneNumber
			};
		}
	}
}