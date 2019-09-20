using CuttingEdge.Conditions;

namespace SquareAccess.Configuration
{
	public class SquareMerchantCredentials
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }

		public SquareMerchantCredentials( string accessToken, string refreshToken )
		{
			Condition.Requires( accessToken, "accessToken" ).IsNotNull();
			Condition.Requires( refreshToken, "refreshToken" ).IsNotNull();

			this.AccessToken = accessToken;
			this.RefreshToken = refreshToken;
		}
	}
}