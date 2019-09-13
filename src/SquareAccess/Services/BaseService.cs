﻿using CuttingEdge.Conditions;
using Newtonsoft.Json;
using SquareAccess.Configuration;
using SquareAccess.Exceptions;
using SquareAccess.Shared;
using SquareAccess.Throttling;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SquareAccess.Services
{
	public class BaseService
	{
		protected SquareConfig Config { get; private set; }
		protected readonly Throttler Throttler;
		protected readonly HttpClient HttpClient;

		private Func< string > _additionalLogInfo;
		private const int _tooManyRequestsHttpCode = 429;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func< string > AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( SquareConfig config )
		{
			Condition.Requires( config, "config" ).IsNotNull();

			this.Config = config;
			this.Throttler = new Throttler( config.ThrottlingOptions.MaxRequestsPerTimeInterval, config.ThrottlingOptions.TimeIntervalInSec, config.ThrottlingOptions.MaxRetryAttempts );

			HttpClient = new HttpClient()
			{
				BaseAddress = new Uri( Config.ApiBaseUrl ) 
			};
		}

		protected async Task< T > PostAsync< T >( string url, Dictionary< string, string > body, CancellationToken cancellationToken, Mark mark = null )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new SquareException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
			}

			var responseContent = await Throttler.ExecuteAsync( () =>
			{
				return new ActionPolicy( Config.NetworkOptions.RetryAttempts, Config.NetworkOptions.DelayBetweenFailedRequestsInSec, Config.NetworkOptions.DelayFailRequestRate )
					.ExecuteAsync(async () =>
						{
							using( var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) )
							{
								SquareLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

								var payload = new FormUrlEncodedContent( body );
								linkedTokenSource.CancelAfter( Config.NetworkOptions.RequestTimeoutMs );

								var httpResponse = await HttpClient.PostAsync( url, payload, linkedTokenSource.Token ).ConfigureAwait( false );
								var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

								SquareLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: content, additionalInfo : this.AdditionalLogInfo() ) );

								ThrowIfError( httpResponse, content );

								return content;
							}
						}, 
						( timeSpan, retryCount ) =>
						{
							string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							SquareLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						SquareLogger.LogTraceException);
			}).ConfigureAwait( false );

			var response = JsonConvert.DeserializeObject< T >( responseContent );

			return response;
		}

		protected void ThrowIfError( HttpResponseMessage response, string message )
		{
			HttpStatusCode responseStatusCode = response.StatusCode;

			if ( response.IsSuccessStatusCode )
				return;

			if ( responseStatusCode == HttpStatusCode.Unauthorized )
			{
				throw new SquareUnauthorizedException( message );
			}
			else if ( (int)responseStatusCode == _tooManyRequestsHttpCode )
			{
				throw new SquareRateLimitsExceeded( message );
			}

			throw new SquareNetworkException( message );
		}

		/// <summary>
		///	Creates method calling detailed information
		/// </summary>
		/// <param name="url">Absolute path to service endpoint</param>
		/// <param name="mark">Unique stamp to track concrete method</param>
		/// <param name="errors">Errors</param>
		/// <param name="methodResult">Service endpoint raw result</param>
		/// <param name="additionalInfo">Extra logging information</param>
		/// <param name="memberName">Method name</param>
		/// <returns></returns>
		protected string CreateMethodCallInfo( string url = "", Mark mark = null, string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			string serviceEndPoint = null;
			string requestParameters = null;

			if ( !string.IsNullOrEmpty( url ) )
			{
				Uri uri = new Uri( url.Contains( Config.ApiBaseUrl ) ? url : Config.ApiBaseUrl + url );

				serviceEndPoint = uri.LocalPath;
				requestParameters = uri.Query;
			}

			var str = string.Format(
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace( serviceEndPoint ) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace( requestParameters ) ? string.Empty : ", RequestParameters: " + requestParameters,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
			);
			return str;
		}

		#region IDisposable Support
		private bool disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					this.Throttler.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
