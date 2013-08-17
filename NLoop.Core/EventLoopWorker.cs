using System;
using System.Threading;
using System.Threading.Tasks;
using NLoop.Core.Utils;

namespace NLoop.Core
{
	/// <summary>
	/// Implements the worker for the <see cref="EventLoop"/>. The worker spins and processes the <see cref="EventLoop.callbackQueue"/>.
	/// </summary>
	public class EventLoopWorker : Disposable
	{
		/// <summary>
		/// Grants the worker 10 seconds to finish work before shutting down.
		/// </summary>
		private static readonly TimeSpan DisposeWaitTimeout = new TimeSpan(0, 0, 10);
		/// <summary>
		/// Gets the next callback from the <see cref="EventLoop.callbackQueue"/>.
		/// </summary>
		private readonly Func<Action> nextCallback;
		/// <summary>
		/// Task on which the callbacks are invoked.
		/// </summary>
		private readonly Task worker;
		/// <summary>
		/// Synchronizes the stopping of the <see cref="worker"/> and the <see cref="EventLoop"/>.
		/// </summary>
		private readonly ManualResetEvent stopHandle = new ManualResetEvent(true);
		/// <summary>
		/// WaitHandle to set if there is more work to be done.
		/// </summary>
		private readonly ManualResetEvent moreWorkHandle = new ManualResetEvent(true);
		/// <summary>
		/// Gets a flag whether this worker is running or not.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public bool IsRunning
		{
			get
			{
				// check if we are not disposed
				CheckDisposed();

				// return the state of the stop handle
				return !stopHandle.WaitOne(0);
			}
		}
		/// <summary>
		/// Gets a flag indicating whether the worker is idling.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public bool IsIdling
		{
			get
			{
				// check if we are not disposed
				CheckDisposed();

				// return the state of the stop handle
				return !moreWorkHandle.WaitOne(0);
			}
		}
		/// <summary>
		/// Constructs a new event loop worker.
		/// </summary>
		/// <param name="nextCallback">A method which to invoke to get the next callack from the <see cref="EventLoop.callbackQueue"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="nextCallback"/> is null.</exception>
		public EventLoopWorker(Func<Action> nextCallback)
		{
			// validate arguments
			if (nextCallback == null)
				throw new ArgumentNullException("nextCallback");

			// keep a reference to the event loop
			this.nextCallback = nextCallback;

			// create the new worker
			worker = Task.Factory.StartNew(() => { });
			worker.Wait();
		}
		/// <summary>
		/// Starts doing work on this <see cref="EventLoopWorker"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public void Start()
		{
			// check if we are not disposed
			CheckDisposed();

			// first clear the stop handle, which enables the worker loop to rung
			stopHandle.Reset();
			SignalMoreWork();

			// start doing th work
			worker.ContinueWith(Work);
		}
		/// <summary>
		/// Stops doing work.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this worker has been disposed of.</exception>
		public void Stop()
		{
			// check if we are not disposed
			CheckDisposed();

			// signal the worker to stop processing
			stopHandle.Set();
		}
		/// <summary>
		/// Signals the worker there is work to be done.
		/// </summary>
		public void SignalMoreWork()
		{
			// if the worker is disposed, do nothing
			if (IsDisposed)
				return;

			// signal worker there is something to do
			moreWorkHandle.Set();
		}
		/// <summary>
		/// Implements the event loop.
		/// </summary>
		/// <param name="incomingTask">The incoming <see cref="Task"/>.</param>
		private void Work(Task incomingTask)
		{
			// validate arguments
			if (incomingTask == null)
				throw new ArgumentNullException("incomingTask");

			// start an endless loop
			while (true)
			{
				// check if the stop handle has been set, indicating we should stop doing work
				if (stopHandle.WaitOne(0))
					break;

				// get a callback from the event loop's callback queue
				var callback = nextCallback();

				// if no callback was returned, there is nothing to do, clear the more work handle and wait for it to be set again
				if (callback == null)
				{
					moreWorkHandle.Reset();
					moreWorkHandle.WaitOne(Timeout.Infinite);
					continue;
				}

				// invoke the callback
				callback();
			}

			// dispose the incoming task
			incomingTask.Dispose();
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

			// check if the worker is running
			if (!stopHandle.WaitOne(0))
			{
				// stop executing work
				stopHandle.Set();
				moreWorkHandle.Reset();

				// wait DisposeWaitTimeout for the worker to finish processing, that should be enough, if not tough luck
				worker.Wait(DisposeWaitTimeout);
			}

			// cleanup
			worker.Dispose();
			stopHandle.Dispose();
			moreWorkHandle.Dispose();
		}
	}
}