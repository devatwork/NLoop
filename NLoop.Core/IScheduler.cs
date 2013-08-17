using System;

namespace NLoop.Core
{
	/// <summary>
	/// Schedules callbacks on a <see cref="EventLoop" />.
	/// </summary>
	public interface IScheduler
	{
		/// <summary>
		/// Schedules a new <paramref name="callback" /> for execution on the event loop.
		/// </summary>
		/// <param name="callback">The callback which to schedule for execution.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		void Schedule(Action callback);
	}
}