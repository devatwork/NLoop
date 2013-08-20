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
		/// Holds the time to wait before this timer invokes its callback should be executed.
		/// </summary>
		private readonly TimeSpan dueTime;
		/// <summary>
		/// Holds the period which to wait before this timer invokes its callback after the previous <see cref="dueTime"/>.
		/// </summary>
		private readonly TimeSpan period;
		/// <summary>
		/// Holds the <see cref="Timer"/>.
		/// </summary>
		private Timer timer;
		/// <summary>
		/// Contructs a new timer control.
		/// </summary>
		/// <param name="dueTime">Holds the time to wait before this timer invokes its callback should be executed.</param>
		public TimerControl(TimeSpan dueTime) : this(dueTime, Timeout.InfiniteTimeSpan)
		{
		}
		/// <summary>
		/// Contructs a new timer control.
		/// </summary>
		/// <param name="dueTime">Holds the time to wait before this timer invokes its callback should be executed.</param>
		/// <param name="period">Holds the period which to wait before this timer invokes its callback after the previous <see cref="dueTime"/>.</param>
		public TimerControl(TimeSpan dueTime, TimeSpan period)
		{
			// store the arguments
			this.dueTime = dueTime;
			this.period = period;
		}
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
		/// Resets this timer. Useful in watchdog situations.
		/// </summary>
		public void Reset()
		{
			// check if the timer is disposed
			if (IsDisposed)
				return;

			// change the timer
			timer.Change(dueTime, period);
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
		/// Sets this timer..
		/// </summary>
		/// <param name="callback">The callback which to invoke on timeout.</param>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public void Set(Action callback)
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
			}, null, dueTime, period);
		}
	}
}