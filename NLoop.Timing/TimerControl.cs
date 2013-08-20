using System;
using System.Threading;
using NLoop.Core.Utils;

namespace NLoop.Timing
{
	/// <summary>
	/// Controls the timer.
	/// </summary>
	public class TimerControl : Disposable, ITimer
	{
		/// <summary>
		/// Holds the <see cref="CancellationTokenSource"/> of this timer.
		/// </summary>
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		/// <summary>
		/// Holds the <see cref="Timer"/>.
		/// </summary>
		private Timer timer;
		/// <summary>
		/// Gets the <see cref="CancellationToken"/> of this timer.
		/// </summary>
		public CancellationToken Token
		{
			get { return cts.Token; }
		}
		/// <summary>
		/// Cancels this timer. The timer will not longer be available.
		/// </summary>
		public void Cancel()
		{
			if (!IsDisposed)
				cts.Cancel();
		}
		/// <summary>
		/// Dispose resources. Override this method in derived classes. Unmanaged resources should always be released
		/// when this method is called. Managed resources may only be disposed of if disposeManagedResources is true.
		/// </summary>
		/// <param name="disposeManagedResources">A value which indicates whether managed resources may be disposed of.</param>
		protected override void DisposeResources(bool disposeManagedResources)
		{
			// we only hold managed resources
			if (!disposeManagedResources)
				return;

			// dispose the timer
			if (timer != null)
				timer.Dispose();

			// dispose the cancellation token
			cts.Dispose();
		}
		/// <summary>
		/// Sets the timer to invoke <paramref name="callback"/> after <see cref="Timeout"/> elapsed.
		/// </summary>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public void SetTimeout(TimeSpan timeout, Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the timer
			timer = new Timer(state => {
				// if the timer was cancelled do not invoke the callback
				if (cts.IsCancellationRequested)
					return;

				// invoke the callback
				callback();
			}, null, timeout, Timeout.InfiniteTimeSpan);
		}
		/// <summary>
		/// Invokes the <paramref name="callback"/> after every elapsed <paramref name="timeout"/>.
		/// </summary>
		/// <param name="timeout">The timeout which to wait before <paramref name="callback"/> should be executed.</param>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public void SetInterval(TimeSpan timeout, Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// create the timer
			timer = new Timer(state => {
				// if the timer was cancelled do not invoke the callback
				if (cts.IsCancellationRequested)
					return;

				// invoke the callback
				callback();
			}, null, timeout, timeout);
		}
	}
}