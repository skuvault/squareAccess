using CuttingEdge.Conditions;

namespace SquareAccess.Configuration
{
	public class SquareConfig
	{
		public string ApplicationId { get; private set; }
		public string ApplicationSecret { get; private set; }
		public string AccessToken { get; private set; }

		/// <summary>
		///	Default page size for orders
		/// </summary>
		public int OrdersPageSize = 10;

		public readonly string ApiBaseUrl = "https://connect.squareup.com";

		public readonly ThrottlingOptions ThrottlingOptions;
		public readonly NetworkOptions NetworkOptions;

		public SquareConfig( string applicationId, string applicationSecret, string accessToken, ThrottlingOptions throttlingOptions, NetworkOptions networkOptions )
		{
			Condition.Requires( applicationId, "applicationId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( applicationSecret, "applicationSecret" ).IsNotNullOrWhiteSpace();
			Condition.Requires( accessToken, "accessToken" ).IsNotNullOrWhiteSpace();
			Condition.Requires( throttlingOptions, "throttlingOptions" ).IsNotNull();
			Condition.Requires( networkOptions, "networkOptions" ).IsNotNull();

			this.ApplicationId = applicationId;
			this.ApplicationSecret = applicationSecret;
			this.AccessToken = accessToken;
			this.ThrottlingOptions = throttlingOptions;
			this.NetworkOptions = networkOptions;
		}

		public SquareConfig( string applicationId, string applicationSecret, string accessToken ) : this( applicationId, applicationSecret, accessToken, ThrottlingOptions.SquareDefaultOptions, NetworkOptions.SquareDefaultOptions )
		{ }
	}

	public class ThrottlingOptions
	{
		public int MaxRequestsPerTimeInterval { get; private set; }
		public int TimeIntervalInSec { get; private set; }
		public int MaxRetryAttempts { get; private set; }

		public ThrottlingOptions( int maxRequests, int timeIntervalInSec, int maxRetryAttempts )
		{
			Condition.Requires( maxRequests, "maxRequests" ).IsGreaterOrEqual( 1 );
			Condition.Requires( timeIntervalInSec, "timeIntervalInSec" ).IsGreaterOrEqual( 1 );
			Condition.Requires( maxRetryAttempts, "maxRetryAttempts" ).IsGreaterOrEqual( 0 );

			this.MaxRequestsPerTimeInterval = maxRequests;
			this.TimeIntervalInSec = timeIntervalInSec;
			this.MaxRetryAttempts = maxRetryAttempts;
		}

		public static ThrottlingOptions SquareDefaultOptions
		{
			get
			{
				return new ThrottlingOptions( 5, 1, 10 );
			}
		}
	}

	public class NetworkOptions
	{
		public int RequestTimeoutMs { get; private set; }
		public int RetryAttempts { get; private set; }
		public int DelayBetweenFailedRequestsInSec { get; private set; }
		public int DelayFailRequestRate { get; private set; }

		public NetworkOptions( int requestTimeoutMs, int retryAttempts, int delayBetweenFailedRequestsInSec, int delayFaileRequestRate )
		{
			Condition.Requires( requestTimeoutMs, "requestTimeoutMs" ).IsGreaterThan( 0 );
			Condition.Requires( retryAttempts, "retryAttempts" ).IsGreaterOrEqual( 0 );
			Condition.Requires( delayBetweenFailedRequestsInSec, "delayBetweenFailedRequestsInSec" ).IsGreaterOrEqual( 0 );
			Condition.Requires( delayFaileRequestRate, "delayFaileRequestRate" ).IsGreaterOrEqual( 0 );

			this.RequestTimeoutMs = requestTimeoutMs;
			this.RetryAttempts = retryAttempts;
			this.DelayBetweenFailedRequestsInSec = delayBetweenFailedRequestsInSec;
			this.DelayFailRequestRate = delayFaileRequestRate;
		}

		public static NetworkOptions SquareDefaultOptions
		{
			get
			{
				return new NetworkOptions( 30 * 1000, 5, 5, 20 );
			}
		}
	}

	public class SquareEndPoint
	{
		public static readonly string GetOAuth2AuthorizationUrl = "/oauth2/authorize";
		public static readonly string ObtainOAuth2TokenUrl = "/oauth2/token";

		public static readonly string OrdersSearchUrl = "/v2/orders/search";		//TODO GUARD-203 Log
		public static readonly string ListLocationsUrl = "/v2/locations";		//TODO GUARD-203 Log
		public static readonly string RetrieveCustomerByIdUrl = "/v2/customers/{customer_id}";	//TODO GUARD-203 Log
		public static readonly string RetrieveCatalogObjectByIdUrl = "/v2/catalog/object/{object_id}";	//TODO GUARD-203 Log

	}
}
