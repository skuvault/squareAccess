using Newtonsoft.Json;
using System;

namespace SquareAccess.Models.Authentication
{
	public class OAuthTokensPair
	{
		[ JsonProperty( "access_token" ) ]
		public string AccessToken { get; set; }
		[ JsonProperty( "token_type" ) ]
		public string TokenType { get; set; }
		[ JsonProperty( "expires_at" ) ]
		public DateTime ExpiresAt { get; set; }
		[ JsonProperty( "merchant_id" ) ]
		public string MerchantId { get; set; }
		[ JsonProperty( "refresh_token" ) ]
		public string RefreshToken { get; set; }
	}
}
