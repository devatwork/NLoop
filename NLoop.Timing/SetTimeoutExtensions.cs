using System;
using System.Threading;
using NLoop.Core;
using NLoop.Core.Utils;

namespace NLoop.Timing
{
	/// <summary>
	/// Contains the timeout extension methods for the <see cref="EventLoop"/>.
	/// </summary>
	public static class SetTimeoutExtensions
	{
		/// <summary>
		/// Invokes the <paramref name="callback"/> after <paramref name="timeout"/> elapsed.
		/// </summary>
		/// <param name="resouceResourceTrackingScheduler">The <see cref="IResourceTrackingScheduler"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="Action"/> which cancels the timeout.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static Action SetTimeout(this IResourceTrackingScheduler resouceResourceTrackingScheduler, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (resouceResourceTrackingScheduler == null)
				throw new ArgumentNullException("resouceResourceTrackingScheduler");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the cancellation token
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// create a resource managed by the event loop
			resouceResourceTrackingScheduler.TrackResource(token, untrack => {
				// create the timer which will schedule the callback
				var timer = new Timer(state => {
					// set timeout only works once, so it does not longer have to be a managed resource
					IDisposable disposer;
					if (!untrack(out disposer))
						return;

					// check if the operation was not cancelled
					if (!token.IsCancellationRequested)
					{
						// schedule the callback for execution
						resouceResourceTrackingScheduler.Schedule(callback);
					}

					// dispose the timer and cts
					disposer.Dispose();
				}, null, timeout, Timeout.InfiniteTimeSpan);

				// dispose all resources
				var dispose = new DisposeAction(() => {
					IDisposable disposer;
					untrack(out disposer);
					timer.Dispose();
					cts.Dispose();
				});

				// untrack the timer resource if it is cancelled
				token.Register(dispose.Dispose);

				// return the dispose action
				return dispose;
			});

			// return an action which cancels the cts
			return cts.Cancel;
		}
		/// <summary>
		/// Invokes the <paramref name="callback"/> after every elapsed <paramref name="timeout"/>.
		/// </summary>
		/// <param name="resouceResourceTrackingScheduler">The <see cref="IResourceTrackingScheduler"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="Action"/> which cancels the interval.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static Action SetInterval(this IResourceTrackingScheduler resouceResourceTrackingScheduler, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (resouceResourceTrackingScheduler == null)
				throw new ArgumentNullException("resouceResourceTrackingScheduler");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the cancellation token
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// create a resource managed by the event loop
			resouceResourceTrackingScheduler.TrackResource(token, untrack => {
				// create the timer which will schedule the callback
				var timer = new Timer(state => {
					// check if the operation was cancelled
					if (token.IsCancellationRequested)
						return;

					// schedule the callback for execution
					resouceResourceTrackingScheduler.Schedule(callback);
				}, null, timeout, timeout);

				// dispose all resources
				var dispose = new DisposeAction(() => {
					IDisposable disposer;
					untrack(out disposer);
					timer.Dispose();
					cts.Dispose();
				});

				// untrack the timer resource if it is cancelled
				token.Register(dispose.Dispose);

				// return the dispose action
				return dispose;
			});

			// return an action which cancels the cts
			return cts.Cancel;
		}
	}
}