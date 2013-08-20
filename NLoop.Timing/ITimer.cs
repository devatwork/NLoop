using NLoop.Core;

namespace NLoop.Timing
{
	/// <summary>
	/// Represents a timer started on a <see cref="EventLoop"/>.
	/// </summary>
	public interface ITimer
	{
		/// <summary>
		/// Cancels this timer. The timer will not longer be available.
		/// </summary>
		void Cancel();
		/// <summary>
		/// Resets this timer. Useful in watchdog situations.
		/// </summary>
		void Reset();
	}
}