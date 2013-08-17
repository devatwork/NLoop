using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLoop.Core
{
	/// <summary>
	/// Implements the worker for the <see cref="EventLoop"/>. The worker spins and processes the <see cref="EventLoop.callbackQueue"/>.
	/// </summary>
	public class EventLoopWorker
	{
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
		private readonly ManualResetEvent stopHandle = new ManualResetEvent(false);
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
		}
		/// <summary>
		/// Starts doing work on this <see cref="EventLoopWorker"/>.
		/// </summary>
		public void Start()
		{
			// first clear the stop handle, which enables the worker loop to rung
			stopHandle.Reset();

			// start doing th work
			worker.ContinueWith(Work);
		}
		/// <summary>
		/// Stops doing work.
		/// </summary>
		public void Stop()
		{
			stopHandle.Set();
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

				// if no callback was returned, there is nothing to do
				// TODO: make this more efficient with a wait handle?
				if (callback == null)
					continue;

				// invoke the callback
				callback();
			}

			// dispose the incoming task
			incomingTask.Dispose();
		}
	}
}