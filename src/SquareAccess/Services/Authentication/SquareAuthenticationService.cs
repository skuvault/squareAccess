using CuttingEdge.Conditions;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Models.Authentication;
using SquareAccess.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[ assembly: InternalsVisibleTo( "SquareAccessTests" ) ]

namespace SquareAccess.Services.Authentication
{
	public sealed class SquareAuthenticationService : BaseService, ISquareAuthenticationService
	{
		public SquareAuthenticationService( SquareConfig config ) : base( config )
		{
		}

		public string GetAuthorizationUrl( SquareOAuthPermission[] scopes, SquareLocale locale = null, bool useActiveUserSession = true, string state = null )
		{
			Condition.Requires( scopes, "scopes" ).IsNotEmpty();

			var scopeParam = string.Join( " ", scopes );
			var url = $"{ base.Config.ApiBaseUrl }{ SquareEndPoint.GetOAuth2AuthorizationUrl }?scope={ scopeParam }&client_id={ base.Config.ApplicationId }";

			if ( locale != null )
			{
				url += $"&locale={ locale }";
			}

			if ( !useActiveUserSession )
			{
				url += "&session=false";
			}

			if ( !string.IsNullOrWhiteSpace( state ) )
			{
				url += $"&state={ state }";
			}

			return url;
		}

		public async Task< OAuthTokensPair > GetTokensAsync( string code, CancellationToken cancellationToken )
		{
			Condition.Requires( code, "code" ).IsNotNullOrWhiteSpace();

			var mark = Mark.CreateNew();
			var requestParameters = new Dictionary< string, string >
			{
				{ "client_id", base.Config.ApplicationId },
				{ "grant_type", "authorization_code" },
				{ "code", code },
			};

			var url = SquareEndPoint.ObtainOAuth2TokenUrl + "?" +  string.Join( "&", requestParameters.Select( item => $"{ item.Key }={ item.Value }" ) );

			try
			{
				SquareLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var body = new Dictionary< string, string >() { { "client_secret", base.Config.ApplicationSecret } };
				var tokens = await base.PostAsync< OAuthTokensPair >( url, body, cancellationToken, mark ).ConfigureAwait( false );

				SquareLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: tokens.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return tokens;
			}
			catch( Exception exception )
			{
				var squareException = new SquareException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}
		}

		public async Task< OAuthTokensPair > RefreshAccessToken( string refreshToken, CancellationToken cancellationToken )
		{
			Condition.Requires( refreshToken, "refreshToken" ).IsNotNullOrWhiteSpace();

			var mark = Mark.CreateNew();
			var requestParameters = new Dictionary< string, string >
			{
				{ "client_id", base.Config.ApplicationId },
				{ "grant_type", "refresh_token" },
				{ "refresh_token", refreshToken },
			};

			var url = SquareEndPoint.ObtainOAuth2TokenUrl + "?" +  string.Join( "&", requestParameters.Select( item => $"{ item.Key }={ item.Value }" ) );

			try
			{
				SquareLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var body = new Dictionary< string, string >() { { "client_secret", base.Config.ApplicationSecret } };
				var tokens = await base.PostAsync< OAuthTokensPair >( url, body, cancellationToken, mark ).ConfigureAwait( false );

				SquareLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: tokens.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return tokens;
			}
			catch( Exception exception )
			{
				var squareException = new SquareException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				SquareLogger.LogTraceException( squareException );
				throw squareException;
			}
		}

		internal async Task< string > GetAuthenticationHtmlForm( string url )
		{
			Condition.Requires( url, "url" ).IsNotNullOrWhiteSpace();

			var httpClient = new HttpClient()
			{
				BaseAddress = new Uri( base.Config.ApiBaseUrl )
			};

			var httpResponse = await httpClient.GetAsync( url ).ConfigureAwait( false );

			return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );
		}
	}

	public class SquareLocale
	{
		private readonly string _localeCode;

		private SquareLocale( string localeCode ) 
		{
			Condition.Requires( localeCode, "localeCode" ).IsNotNullOrWhiteSpace();

			this._localeCode = localeCode;
		}

		public override string ToString()
		{
			return this._localeCode;
		}

		public static SquareLocale EnUS => new SquareLocale( "en-US" );
		public static SquareLocale EnCA => new SquareLocale( "en-CA" );
		public static SquareLocale EsUS => new SquareLocale( "es-US" );
		public static SquareLocale FrCA => new SquareLocale( "fr-CA" );
		public static SquareLocale JaJP => new SquareLocale( "ja-JP" );
	}

	public static class SquareOAuthPermissions
	{
		/// <summary>
		///	Returns permissions to work with items, their inventory and orders
		/// </summary>
		/// <returns></returns>
		public static SquareOAuthPermission[] GetDefault()
		{
			return new SquareOAuthPermission[] { SquareOAuthPermission.INVENTORY_READ,
									 SquareOAuthPermission.INVENTORY_WRITE, 
									 SquareOAuthPermission.ITEMS_READ,
									 SquareOAuthPermission.ITEMS_WRITE, 
									 SquareOAuthPermission.ORDERS_READ, 
									 SquareOAuthPermission.MERCHANT_PROFILE_READ,
									 SquareOAuthPermission.CUSTOMERS_READ };
		}
	}

	/// <summary>
	///	Application OAuth permissions
	/// </summary>
	public enum SquareOAuthPermission
	{
		/// <summary>
		///	Grants read access to bank account information associated with the targeted Square account. For example, to call the Connect v1 ListBankAccounts endpoint.
		/// </summary>
		BANK_ACCOUNTS_READ,
		/// <summary>
		///	Grants read access to customer information. For example, to call the ListCustomers endpoint
		/// </summary>
		CUSTOMERS_READ,
		/// <summary>
		///	Grants write access to customer information. For example, to create and update customer profiles
		/// </summary>
		CUSTOMERS_WRITE,
		/// <summary>
		///	Grants read access to employee profile information. For example, to call the Connect v1 Employees API
		/// </summary>
		EMPLOYEES_READ,
		/// <summary>
		///	Grants write access to employee profile information. For example, to create and modify employee profiles
		/// </summary>
		EMPLOYEES_WRITE,
		/// <summary>
		///	Grants read access to inventory information. For example, to call the RetrieveInventoryCount endpoint
		/// </summary>
		INVENTORY_READ,
		/// <summary>
		///	Grants write access to inventory information. For example, to call the BatchChangeInventory endpoint
		/// </summary>
		INVENTORY_WRITE,
		/// <summary>
		///	Grants read access to product catalog information. For example, to get an item or a list of items
		/// </summary>
		ITEMS_READ,
		/// <summary>
		///	Grants write access to product catalog information. For example, to modify or add to a product catalog
		/// </summary>
		ITEMS_WRITE,
		/// <summary>
		///	Grants read access to business and location information. For example, to obtain a location ID for subsequent activity
		/// </summary>
		MERCHANT_PROFILE_READ,
		/// <summary>
		///	Grants read access to order information. For example, to call the BatchRetrieveOrders endpoint
		/// </summary>
		ORDERS_READ,
		/// <summary>
		///	Grants write access to order information. For example, to call the CreateCheckout endpoint
		/// </summary>
		ORDERS_WRITE,
		/// <summary>
		///	Grants read access to transaction and refund information. For example, to call the RetrieveTransaction endpoint
		/// </summary>
		PAYMENTS_READ,
		/// <summary>
		///	Grants write access to transaction and refunds information. For example, to process payments with the Transactions or Checkout API
		/// </summary>
		PAYMENTS_WRITE,
		/// <summary>
		///	Allow third party applications to deduct a portion of each transaction amount. Required to use multiparty transaction functionality with the Transactions API
		/// </summary>
		PAYMENTS_WRITE_ADDITIONAL_RECIPIENTS,
		/// <summary>
		///	Grants write access to transaction and refunds information. For example, to process in-person payments
		/// </summary>
		PAYMENTS_WRITE_IN_PERSON,
		/// <summary>
		///	Grants read access to settlement (deposit) information. For example, to call the Connect v1 ListSettlements endpoint
		/// </summary>
		SETTLEMENTS_READ,
		/// <summary>
		///	Grants read access to employee timecard information. For example, to call the Connect v1 ListTimecards endpoint
		/// </summary>
		TIMECARDS_READ,
		/// <summary>
		///	Grants write access to employee timecard information. For example, to create and modify timecards
		/// </summary>
		TIMECARDS_WRITE,
		/// <summary>
		///	Grants read access to employee timecard settings information. For example, to call the GetBreakType endpoint
		/// </summary>
		TIMECARDS_SETTINGS_READ,
		/// <summary>
		///	Grants write access to employee timecard settings information. For example, to call the UpdateBreakType endpoint.
		/// </summary>
		TIMECARDS_SETTINGS_WRITE
	}
}
