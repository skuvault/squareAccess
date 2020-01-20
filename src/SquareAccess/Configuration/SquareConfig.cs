using CuttingEdge.Conditions;

namespace SquareAccess.Configuration
{
	public class SquareConfig
	{
		public string ApplicationId { get; private set; }
		public string ApplicationSecret { get; private set; }

		/// <summary>
		///	Default page size for orders
		/// </summary>
		public int OrdersPageSize = 10;

		private readonly bool IsSandbox;

		public string ApiBaseUrl { 
			get { return ( !IsSandbox ) ? "https://connect.squareup.com" : "https://connect.squareupsandbox.com"; }
		}

		public readonly ThrottlingOptions ThrottlingOptions;
		public readonly NetworkOptions NetworkOptions;

		public SquareConfig( string applicationId, string applicationSecret, ThrottlingOptions throttlingOptions, NetworkOptions networkOptions, bool isSandbox )
		{
			Condition.Requires( applicationId, "applicationId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( applicationSecret, "applicationSecret" ).IsNotNullOrWhiteSpace();
			Condition.Requires( throttlingOptions, "throttlingOptions" ).IsNotNull();
			Condition.Requires( networkOptions, "networkOptions" ).IsNotNull();

			this.ApplicationId = applicationId;
			this.ApplicationSecret = applicationSecret;
			this.ThrottlingOptions = throttlingOptions;
			this.NetworkOptions = networkOptions;
			this.IsSandbox = isSandbox;
		}

		public SquareConfig( string applicationId, string applicationSecret, bool isSandbox ) : this( applicationId, applicationSecret, ThrottlingOptions.SquareDefaultOptions, NetworkOptions.SquareDefaultOptions, isSandbox )
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
				return new ThrottlingOptions( 4, 1, 10 );
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
				return new NetworkOptions( 5 * 60 * 1000, 10, 5, 20 );
			}
		}
	}

	public class SquareEndPoint
	{
		public static readonly string GetOAuth2AuthorizationUrl = "/oauth2/authorize";
		public static readonly string ObtainOAuth2TokenUrl = "/oauth2/token";
		public static readonly string SearchCatalogUrl = "/v2/catalog/search";
		public static readonly string RetrieveInventoryCounts = "/v2/inventory/batch-retrieve-counts";
		public static readonly string BatchChangeInventory = "/v2/inventory/batch-change";
		public static readonly string OrdersSearchUrl = "/v2/orders/search";
		public static readonly string ListLocationsUrl = "/v2/locations";
		public static readonly string RetrieveCustomerByIdUrl = "/v2/customers/{customer_id}";
		public static readonly string BatchRetrieveCatalogObjectsUrl = "/v2/catalog/batch-retrieve";
	}
}
