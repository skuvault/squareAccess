using SquareAccess.Configuration;
using SquareAccess.Models.Authentication;
using System.Threading.Tasks;

namespace SquareAccess.Services.Authentication
{
	public interface ISquareAuthenticationService
	{
		/// <summary>
		///	Build OAuth2 application authorization url with specified scopes 
		/// </summary>
		/// <param name="scopes">List of the permissions the application is requesting</param>
		/// <param name="locale">The locale to present the permission request form in. Square detects the appropriate locale automatically. Only provide this value if the application can definitively determine the preferred locale. Currently supported values: en-US, en-CA, es-US, fr-CA, ja-JP</param>
		/// <param name="useActiveUserSession">If false, the user must log in to their Square account to view the Permission Request form, even if they already have a valid user session</param>
		/// <param name="state">When provided, state is passed along to the configured Redirect URL after the Permission Request form is submitted. You can include state and verify its value to help protect against cross-site request forgery</param>
		/// <returns>url</returns>
		string GetAuthorizationUrl( SquareOAuthPermission[] scopes, SquareLocale locale = null, bool useActiveUserSession = true, string state = null );
	
		/// <summary>
		///	Get OAuth2 new access and refresh tokens using code
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		Task< OAuthTokensPair > GetTokensAsync( string code );

		/// <summary>
		///	Generate new access token instead of expired using refresh token
		/// </summary>
		/// <param name="refreshToken"></param>
		/// <returns></returns>
		Task< OAuthTokensPair > RefreshAccessToken( string refreshToken );
	}
}
