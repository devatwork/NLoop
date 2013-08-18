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
		/// <param name="resourceFactory">Creates the new resource.</param>
		/// <returns>Returns the created <see cref="CancellationTokenSource"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		void TrackResource(CancellationToken token, ResourceFactoryCallback resourceFactory);
	}
}