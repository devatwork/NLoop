using System;
using System.Collections.Concurrent;
using System.Threading;
using NLoop.Core.Utils;

namespace NLoop.Core
{
	/// <summary>
	/// Implements the core API of the event loop.
	/// </summary>
	public class EventLoop : Disposable, IResourceTrackingScheduler
	{
		/// <summary>
		/// Holds all the registered resources used in this event loop.
		/// </summary>
		private readonly ConcurrentDictionary<CancellationToken, IDisposable> resources = new ConcurrentDictionary<CancellationToken, IDisposable>();
		/// <summary>
		/// Holds the <see cref="EventLoopWorker"/> used by this event loop.
		/// </summary>
		private readonly EventLoopWorker worker = new EventLoopWorker();
		/// <summary>
		/// Schedules a new <paramref name="callback" /> for execution on this event loop.
		/// </summary>
		/// <param name="callback">The callback which to schedule for execution.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		public void Schedule(Action callback)
		{
			worker.Schedule(callback);
		}
		/// <summary>
		/// Registers a tracked resource to this event loop. The tracked resource will be disposed of if the event loop is disposed off.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> for the resource.</param>
		/// <param name="resource">The resource which to track.</param>
		/// <returns>Returns the <see cref="UntrackResourceCallback"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the resource could not be tracked.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <see cref="IResourceTrackingScheduler"/> was disposed.</exception>
		public UntrackResourceCallback TrackResource(CancellationToken token, IDisposable resource)
		{
			// validate arguments
			if (token == null)
				throw new ArgumentNullException("token");
			if (resource == null)
				throw new ArgumentNullException("resource");

			// check if we are not disposed
			CheckDisposed();

			// track the resource
			if (!resources.TryAdd(token, resource))
				throw new InvalidOperationException("Failed to track resource");

			return TryUnregisterResource;
		}
		/// <summary>
		/// Starts this event loop and add the given <paramref name="callback"/> to it to execute first.
		/// </summary>
		/// <param name="callback">The callback which to execute on the started event loop.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public void Start(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// add the initial callback to this loop
			Schedule(callback);
		}
		/// <summary>
		/// Dispose resources. Override this method in derived classes. Unmanaged resources should always be released
		/// when this method is called. Managed resources may only be disposed of if disposeManagedResources is true.
		/// </summary>
		/// <param name="disposeManagedResources">A value which indicates whether managed resources may be disposed of.</param>
		protected override void DisposeResources(bool disposeManagedResources)
		{
			// we only have managed resources
			if (!disposeManagedResources)
				return;

			// dispose the worker
			if (worker != null)
				worker.Dispose();

			// dispose all resources tracked by this event loop
			foreach (var disposable in resources.Values)
				disposable.Dispose();
			resources.Clear();
		}
		/// <summary>
		/// Unregisters a resource by its <paramref name="token"/>.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> of the resource.</param>
		private bool TryUnregisterResource(CancellationToken token)
		{
			// unregister the resource.
			IDisposable resource;
			return resources.TryRemove(token, out resource);
		}
	}
}