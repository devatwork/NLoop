using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NLoop
{
	/// <summary>
	/// Implements the core API of the event loop.
	/// </summary>
	public class EventLoop
	{
		/// <summary>
		/// Holds all the callbacks to event handlers which need to be invoked in this event loop.
		/// </summary>
		private readonly ConcurrentQueue<Action> callbackQueue = new ConcurrentQueue<Action>();
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
		public bool WasStarted
		{
			get { return Thread.VolatileRead(ref startedState) == 1; }
		}
		/// <summary>
		/// Adds a new <paramref name="callback" /> to the event loop.
		/// </summary>
		/// <param name="callback">The callback which to add to this event loop.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		public void Add(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// enqueue the callback
			callbackQueue.Enqueue(callback);
		}
		/// <summary>
		/// Starts this event loop and add the given <paramref name="callback"/> to it to execute first.
		/// </summary>
		/// <param name="callback"></param>
		public void Start(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// Attempt to move the started state from 0 to 1. If successful, we can be assured that
			// this thread is the first thread to do so, and can safely start this event loop.
			if (Interlocked.CompareExchange(ref startedState, 1, 0) != 0)
				throw new InvalidOperationException("This eventloop was already started");

			// add the initial callback to this loop
			Add(callback);

			// create the event loop worker
			worker = new EventLoopWorker(NextCallback);

			// start the worker
			worker.Start();
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
	}
}