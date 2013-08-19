using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLoop.Core.Utils;

namespace NLoop.Core
{
	/// <summary>
	/// Implements the worker for the <see cref="EventLoop"/>. The worker spins and processes the callbacks from <see cref="EventLoopWorker.queue"/>.
	/// </summary>
	public class EventLoopWorker : Disposable, IScheduler
	{
		/// <summary>
		/// Grants the worker 10 seconds to finish work before shutting down.
		/// </summary>
		private static readonly TimeSpan DisposeWaitTimeout = TimeSpan.FromSeconds(10);
		/// <summary>
		/// Cancels the processing of callbacks.
		/// </summary>
		private readonly CancellationTokenSource cancelProcessing = new CancellationTokenSource();
		/// <summary>
		/// Holds all the callbacks to event handlers which need to be invoked in this event loop.
		/// </summary>
		private readonly BlockingCollection<Action> queue = new BlockingCollection<Action>();
		/// <summary>
		/// Task on which the callbacks are invoked.
		/// </summary>
		private readonly Task worker;
		/// <summary>
		/// Constructs a new event loop worker.
		/// </summary>
		public EventLoopWorker()
		{
			// create the new worker
			worker = Task.Run(() => Work());
		}
		/// <summary>
		/// Schedules a new <paramref name="callback" /> for execution on the event loop.
		/// </summary>
		/// <param name="callback">The callback which to schedule for execution.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		public void Schedule(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// check if disposed
			if (IsDisposed)
				return;

			// add the callback to the queue
			queue.Add(callback);
		}
		/// <summary>
		/// Implements the event loop.
		/// </summary>
		private void Work()
		{
			try
			{
				// start an endless loop
				var token = cancelProcessing.Token;
				while (true)
				{
					// blocking: get a callback from the event loop's callback queue
					Action callback;
					if (!queue.TryTake(out callback, Timeout.Infinite, token))
						return;

					// invoke the callback if there is one
					if (callback != null)
						callback();
				}
			}
			catch (OperationCanceledException)
			{
				// it is expected
			}
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

			// cancel 
			cancelProcessing.Cancel();

			// block until the worker has finished or dispose timeout happens
			worker.Wait(DisposeWaitTimeout);

			// cleanup
			worker.Dispose();
			cancelProcessing.Dispose();
			queue.Dispose();
		}
	}
}