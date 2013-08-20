using System;
using NLoop.Core;

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
		/// <returns>Returns a <see cref="ITimer"/> which controls the timer.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static ITimer SetTimeout(this IResourceTrackingScheduler resouceResourceTrackingScheduler, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (resouceResourceTrackingScheduler == null)
				throw new ArgumentNullException("resouceResourceTrackingScheduler");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the timer which will schedule the callback
			var timer = new TimerControl();
			var token = timer.Token;

			// create a resource managed by the event loop
			var untrack = resouceResourceTrackingScheduler.TrackResource(token, timer);

			// dispose the timer if cancelled
			token.Register(() => {
				// untrack the resource
				untrack(token);

				// dispose the timer
				timer.Dispose();
			});

			// set the timer
			timer.SetTimeout(timeout, () => {
				// untrack the resource
				untrack(timer.Token);

				// dispose the timer
				timer.Dispose();

				// schedule the callback for execution
				resouceResourceTrackingScheduler.Schedule(callback);
			});

			// return the timer
			return timer;
		}
		/// <summary>
		/// Invokes the <paramref name="callback"/> after every elapsed <paramref name="timeout"/>.
		/// </summary>
		/// <param name="resouceResourceTrackingScheduler">The <see cref="IResourceTrackingScheduler"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="ITimer"/> which controls the timer.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static ITimer SetInterval(this IResourceTrackingScheduler resouceResourceTrackingScheduler, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (resouceResourceTrackingScheduler == null)
				throw new ArgumentNullException("resouceResourceTrackingScheduler");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the timer which will schedule the callback
			var timer = new TimerControl();
			var token = timer.Token;

			// create a resource managed by the event loop
			var untrack = resouceResourceTrackingScheduler.TrackResource(token, timer);

			// dispose the timer if cancelled
			token.Register(() => {
				// untrack the resource
				untrack(token);

				// dispose the timer
				timer.Dispose();
			});

			// set the timer
			timer.SetInterval(timeout, () => resouceResourceTrackingScheduler.Schedule(callback));

			// return the timer
			return timer;
		}
	}
}