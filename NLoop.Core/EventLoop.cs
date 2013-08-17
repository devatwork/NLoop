using System;
using System.Collections.Concurrent;
using System.Threading;
using NLoop.Core.Utils;

namespace NLoop.Core
{
	/// <summary>
	/// Implements the core API of the event loop.
	/// </summary>
	public class EventLoop : Disposable
	{
		/// <summary>
		/// Holds all the callbacks to event handlers which need to be invoked in this event loop.
		/// </summary>
		private readonly ConcurrentQueue<Action> callbackQueue = new ConcurrentQueue<Action>();
		/// <summary>
		/// Holds all the registered resources used in this event loop.
		/// </summary>
		private readonly ConcurrentDictionary<CancellationToken, IDisposable> resources = new ConcurrentDictionary<CancellationToken, IDisposable>();
		/// <summary>
		/// Holds the <see cref="EventLoopWorker"/> used by this event loop.
		/// </summary>
		private EventLoopWorker worker;
		/// <summary>
		/// A value which indicates the started state. 0 indicates not started, 1 indicates starting or started.
		/// </summary>
		private int startedState;
		/// <summary>
		/// Gets a flag indicating whether this event loop was already started or not.
		/// </summary>
		public bool IsStarted
		{
			get { return Thread.VolatileRead(ref startedState) == 1; }
		}
		/// <summary>
		/// Adds a new <paramref name="callback" /> to the event loop.
		/// </summary>
		/// <param name="callback">The callback which to add to this event loop.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public void Add(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// enqueue the callback
			callbackQueue.Enqueue(callback);

			// signal the worker there is more  work
			if (IsStarted && worker != null)
				worker.SignalMoreWork();
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

			// check if we are not disposed
			CheckDisposed();

			// add the initial callback to this loop
			Add(callback);

			// Attempt to move the started state from 0 to 1. If successful, we can be assured that
			// this thread is the first thread to do so, and can safely create a worker for this event loop
			if (Interlocked.CompareExchange(ref startedState, 1, 0) == 0)
			{
				// create the event loop worker
				worker = new EventLoopWorker(NextCallback);
			}

			// start the worker
			worker.Start();
		}
		/// <summary>
		/// Stops this event loop. 
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public void Stop()
		{
			// check if we are not disposed
			CheckDisposed();

			// if the event loop was not started there is not much to do
			if (!IsStarted || worker == null)
				return;

			// tell the worker to stop
			worker.Stop();
		}
		/// <summary>
		/// Registers a new cancellable resource to this event loop.
		/// </summary>
		/// <param name="resourceFactory">Creates the new resource.</param>
		/// <returns>Returns the created <see cref="CancellationTokenSource"/>.</returns>
		public CancellationTokenSource TrackResource(ResourceFactoryCallback resourceFactory)
		{
			// validate arguments
			if (resourceFactory == null)
				throw new ArgumentNullException("resourceFactory");

			// check if we are not disposed
			CheckDisposed();

			// create a cancellation token source used to cancel the resource
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// track the resource
			resources.AddOrUpdate(token, key => {
				// create the resource
				var resource = resourceFactory(token, () => UnregisterResource(token));

				// create a dispose action which disposed the resource and the cancellation token
				return new DisposeAction(() => {
					resource.Dispose();
					cts.Dispose();
				});
			}, (key, value) => { throw new NotSupportedException("The token was already registered for another resource, this should never happen"); });

			// return the created cts
			return cts;
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

			// tell the worker to stop
			if (worker != null)
				worker.Stop();

			// dispose all resources tracked by this event loop
			foreach (var disposable in resources.Values)
				disposable.Dispose();
			resources.Clear();

			// dispose the worker
			if (worker != null)
				worker.Dispose();
		}
		/// <summary>
		/// Tries to dequeue the next callback from the <see cref="callbackQueue"/>.
		/// </summary>
		/// <returns>Returns the callback if there is one, otherwise null.</returns>
		private Action NextCallback()
		{
			Action callback;
			return callbackQueue.TryDequeue(out callback) ? callback : null;
		}
		/// <summary>
		/// Unregisters a resource by its <paramref name="token"/>.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> of the resource.</param>
		private void UnregisterResource(CancellationToken token)
		{
			// invoke the dispose action, if there is one registered for the given token.
			IDisposable disposeAction;
			if (resources.TryRemove(token, out disposeAction))
				disposeAction.Dispose();
		}
	}
}