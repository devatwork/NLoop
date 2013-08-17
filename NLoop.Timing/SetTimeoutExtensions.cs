using System;
using System.Threading;
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
		/// <param name="eventLoop">The <see cref="EventLoop"/> on which the timeout event will be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <returns>Returns a <see cref="IDisposable"/> which cancels the timer.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static IDisposable SetTimeout(this EventLoop eventLoop, Action callback, TimeSpan timeout)
		{
			// validate arguments
			if (eventLoop == null)
				throw new ArgumentNullException("eventLoop");
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the timeout
			return new Timer(state => eventLoop.Add(callback), null, timeout, Timeout.InfiniteTimeSpan);
		}
	}
}