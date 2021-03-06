﻿using FluentAssertions;
using NUnit.Framework;
using SquareAccess.Services.Authentication;
using System.Threading;

namespace SquareAccessTests
{
	public sealed class AuthTests : BaseTest
	{
		private SquareAuthenticationService _authenticationService;
		private string _authorizationCode = "sq0cgp-BoBUIP_Koi9KRdr0xMK4rg";
		private string _applicationId;

		[ SetUp ]
		public void Init()
		{
			this._authenticationService = new SquareAuthenticationService( this.Config );
			this._applicationId = this.Config.ApplicationId;
		}

		[ Test ]
		public void GetAuthorizationUrl()
		{
			var url = this._authenticationService.GetAuthorizationUrl( SquareOAuthPermissions.GetDefault() );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "Sign In" );
		}

		[ Test ]
		public void GetAuthorizationUrlWhenEsLocaleIsSpecified()
		{
			var url = this._authenticationService.GetAuthorizationUrl( SquareOAuthPermissions.GetDefault(), SquareLocale.EsUS );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "Inicia sesión" );
		}

		[ Test ]
		public void GetAuthorizationUrlWhenJpLocaleIsSpecified()
		{
			var url = this._authenticationService.GetAuthorizationUrl( SquareOAuthPermissions.GetDefault(), SquareLocale.JaJP );
			var htmlForm = this._authenticationService.GetAuthenticationHtmlForm( url ).Result;

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( "ログイン" );
		}

		[ Test ]
		public void GetPermanentTokens()
		{
			var tokens = this._authenticationService.GetTokensAsync( this._authorizationCode, CancellationToken.None ).Result;

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
