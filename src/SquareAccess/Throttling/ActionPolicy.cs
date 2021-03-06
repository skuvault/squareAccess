﻿using CuttingEdge.Conditions;
using Polly;
using SquareAccess.Exceptions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Square.Connect.Client;
using System.Net;

namespace SquareAccess.Throttling
{
	public class ActionPolicy
	{
		private readonly int _retryAttempts;
		private readonly int _delay;
		private readonly int _delayRate;
		private readonly string requestIsUnauthorizedErrorMessage = "unable to authorize this request";

		public ActionPolicy( int attempts, int delay, int delayRate )
		{
			Condition.Requires( attempts ).IsGreaterThan( 0 );
			Condition.Requires( delay ).IsGreaterOrEqual( 0 );
			Condition.Requires( delayRate ).IsGreaterOrEqual( 0 );

			this._retryAttempts = attempts;
			this._delay = delay;
			this._delayRate = delayRate;
		}

		/// <summary>
		///	Retries function until it succeed or failed
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="funcToThrottle"></param>
		/// <param name="onRetryAttempt">Retry attempts</param>
		/// <param name="extraLogInfo"></param>
		/// <param name="onException"></param>
		/// <returns></returns>
		public Task< TResult > ExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle, Action< Exception, TimeSpan, int > onRetryAttempt, Func< string > extraLogInfo, Action< Exception > onException )
		{
			return Policy.Handle< SquareNetworkException >()
				.WaitAndRetryAsync( _retryAttempts,
					retryCount => TimeSpan.FromSeconds( this.GetDelayBeforeNextAttempt( retryCount ) ),
					( exception, timeSpan, retryCount, context ) =>
					{
						onRetryAttempt?.Invoke( exception, timeSpan, retryCount );
					})
				.ExecuteAsync( async () =>
				{
					try
					{
						return await funcToThrottle().ConfigureAwait( false );
					}
					catch ( Exception exception )
					{
						if ( exception is SquareNetworkException )
							throw exception;

						SquareException squareException = null;

						var exceptionDetails = string.Empty;

						if ( extraLogInfo != null )
							exceptionDetails = extraLogInfo();

						if ( exception is ApiException )
						{
							var squareApiException = (ApiException)exception;

							if ( squareApiException.ErrorCode == (int)HttpStatusCode.Unauthorized
								|| ( squareApiException.ErrorContent != null && squareApiException.ErrorContent.Contains( requestIsUnauthorizedErrorMessage ) ) )
							{
								exception = new SquareUnauthorizedException( squareApiException.ErrorContent );
							}
						}

						if ( exception is HttpRequestException || exception is SquareUnauthorizedException )
							squareException = new SquareNetworkException( exceptionDetails, exception );
						else
						{
							squareException = new SquareException( exceptionDetails, exception );
							onException?.Invoke( squareException );
						}

						throw squareException;
					}
				});
		}

		public int GetDelayBeforeNextAttempt( int retryCount )
		{
			return this._delay + this._delayRate * retryCount;
		}
	}
}
