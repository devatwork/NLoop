using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLoop.Core;
using NLoop.Core.Promises;

namespace NLoop.Net.Http
{
	/// <summary>
	/// Provides extensions to <see cref="HttpClient"/> to do evented I/O. 
	/// </summary>
	public static class HttpClientExtensions
	{
		/// <summary>
		/// Sends a <paramref name="request"/> using the given <paramref name="client"/>.
		/// </summary>
		/// <param name="client">The <see cref="HttpClient"/> on which to execute the request.</param>
		/// <param name="eventLoop">The <see cref="EventLoop"/> on which to execute callbacks.</param>
		/// <param name="request">The <see cref="HttpResponseMessage"/> which to send.</param>
		/// <returns>Returns a <see cref="Promise{HttpResponseMessage}"/> resolving to <see cref="HttpResponseMessage"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the arguments is null.</exception>
		public static CancelablePromise<HttpResponseMessage> Send(this HttpClient client, EventLoop eventLoop, HttpRequestMessage request)
		{
			// validate arguments
			if (client == null)
				throw new ArgumentNullException("client");
			if (eventLoop == null)
				throw new ArgumentNullException("eventLoop");
			if (request == null)
				throw new ArgumentNullException("request");

			// create the cancellation token
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// create the deferred
			var deferred = eventLoop.Defer<HttpResponseMessage>(cts.Cancel);

			// create a resource managed by the event loop
			eventLoop.TrackResource(token, untrack => Task.Run(() => {
				try
				{
					// send the request and retrieve the response
					var response = client.SendAsync(request, token).Result;

					// resolve the deferred if not cancelled
					if (!token.IsCancellationRequested)
						deferred.Resolve(response);
				}
				catch (Exception ex)
				{
					// something bad happened, reject the deferred if not cancelled
					if (!token.IsCancellationRequested)
						deferred.Reject(ex);
				}

				// dispose the used resources
				cts.Dispose();
			}));

			// return the promise
			return deferred.Promise;
		}
	}
}