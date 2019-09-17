using FluentAssertions;
using NUnit.Framework;
using SquareAccess.Services.Authentication;
using System.Threading;

namespace SquareAccessTests
{
	public sealed class AuthTests : BaseTest
	{
		private SquareAuthenticationService _authenticationService;
		private SquareOAuthPermission[] _defaultScopes = new SquareOAuthPermission[] { SquareOAuthPermission.INVENTORY_WRITE, SquareOAuthPermission.ITEMS_WRITE, SquareOAuthPermission.ORDERS_READ, SquareOAuthPermission.MERCHANT_PROFILE_READ, SquareOAuthPermission.ITEMS_READ, SquareOAuthPermission.INVENTORY_READ };
		private string _applicationId;

		[ SetUp ]
		public void Init()
		{
			this._authenticationService = new SquareAuthenticationService( this.Config );
			this._applicationId = this.Credentials.ApplicationId;
		}

		[ Test ]
		public void GetAuthorizationUrl()
		{
			var url = this._authenticationService.GetAuthorizationUrl( this._defaultScopes );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "Sign In" );
		}

		[ Test ]
		public void GetAuthorizationUrlWhenEsLocaleIsSpecified()
		{
			var url = this._authenticationService.GetAuthorizationUrl( this._defaultScopes, SquareLocale.EsUS );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "Inicia sesión" );
		}

		[ Test ]
		public void GetAuthorizationUrlWhenJpLocaleIsSpecified()
		{
			var url = this._authenticationService.GetAuthorizationUrl( this._defaultScopes, SquareLocale.JaJP );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "ログイン" );
		}

		[ Test ]
		public void GetPermanentTokens()
		{
			var tokens = this._authenticationService.GetTokensAsync( this.Credentials.AuthorizationCode, CancellationToken.None ).Result;

			tokens.Should().NotBeNull();
			tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
			tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void RefreshAccessToken()
		{
			var currentAccessToken = this.Credentials.AccessToken;

			var newTokens = this._authenticationService.RefreshAccessToken( this.Credentials.RefreshToken, CancellationToken.None ).Result;

			newTokens.Should().NotBeNull();
			newTokens.AccessToken.Should().NotBeNullOrWhiteSpace();
			newTokens.AccessToken.Should().NotBe( currentAccessToken );
		}
	}
}
