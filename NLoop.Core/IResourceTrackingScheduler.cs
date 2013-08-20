using System;
using System.Threading;

namespace NLoop.Core
{
	/// <summary>
	/// Schedules callbacks on a <see cref="EventLoop" /> and tracks its resources.
	/// </summary>
	public interface IResourceTrackingScheduler : IScheduler
	{
		/// <summary>
		/// Registers a tracked resource to this event loop. The tracked resource will be disposed of if the event loop is disposed off.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> for the resource.</param>
		/// <param name="resource">The resource which to track.</param>
		/// <returns>Returns the <see cref="UntrackResourceCallback"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the resource could not be tracked.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <see cref="IResourceTrackingScheduler"/> was disposed.</exception>
		UntrackResourceCallback TrackResource(CancellationToken token, IDisposable resource);
	}
}