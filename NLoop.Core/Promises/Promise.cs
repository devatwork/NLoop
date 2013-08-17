using System;
using System.Collections.Generic;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Placeholder for a value which is might not be known initially.
	/// </summary>
	/// <typeparam name="TValue">The type of value.</typeparam>
	public class Promise<TValue>
	{
		/// <summary>
		/// Enumerates the different states a promise can be in.
		/// </summary>
		private enum PromiseState
		{
			/// <summary>
			/// The promise is not fullfilled, it is waiting to be iether <see cref="Resolved"/> or <see cref="Rejected"/>.
			/// </summary>
			Unfulfilled = 0,
			/// <summary>
			/// The promise has been resolved, meaning the result is available.
			/// </summary>
			Resolved = 1,
			/// <summary>
			/// The promise has been rejected, meaning an error occurred.
			/// </summary>
			Rejected = 2
		}
		/// <summary>
		/// Holds all the registered <see cref="ResolvedCallback{TResult}"/>.
		/// </summary>
		private readonly Queue<ResolvedCallback<TValue>> resolvedCallbacks = new Queue<ResolvedCallback<TValue>>();
		/// <summary>
		/// Holds all the registered <see cref="RejectedCallback"/>.
		/// </summary>
		private readonly Queue<RejectedCallback> rejectedCallbacks = new Queue<RejectedCallback>();
		/// <summary>
		/// Holds a reference to the <see cref="IScheduler"/> on which callbacks will be executed.
		/// </summary>
		private readonly IScheduler scheduler;
		/// <summary>
		/// Holds the state of this promise, <see cref="PromiseState"/> for candidates.
		/// </summary>
		private PromiseState state = PromiseState.Unfulfilled;
		/// <summary>
		/// Used for synchronizing <see cref="state"/> changes.
		/// </summary>
		private readonly object stateSyncRoot = new object();
		/// <summary>
		/// Holds the <typeparamref name="TValue"/> if this promise was <see cref="PromiseState.Resolved"/>.
		/// </summary>
		private TValue resolvedValue;
		/// <summary>
		/// Holds the <see cref="Exception"/> if this promise was <see cref="PromiseState.Rejected"/>.
		/// </summary>
		private Exception rejectionReason;
		/// <summary>
		/// Constructs a new <see cref="Promise{T}"/>.
		/// </summary>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to execute callbacks.</param>
		internal Promise(IScheduler scheduler)
		{
			// validate arguments
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			// store
			this.scheduler = scheduler;
		}
		/// <summary>
		/// Resolves this promise with the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The <typeparamref name="TValue"/>.</param>
		internal void Resolve(TValue value)
		{
			// make sure we are the only thread updating the state of this promise
			IEnumerable<ResolvedCallback<TValue>> callbacks;
			lock (stateSyncRoot)
			{
				// set the state to resolved
				state = PromiseState.Resolved;

				// store the result
				resolvedValue = value;

				// get the callbacks currently registered
				callbacks = resolvedCallbacks.ToArray();
				resolvedCallbacks.Clear();
			}

			// schedule all queued callbacks
			foreach (var callback in callbacks)
			{
				var callback1 = callback;
				scheduler.Schedule(() => callback1(value));
			}
		}
		/// <summary>
		/// Rejects this promise with the given <paramref name="reason"/>.
		/// </summary>
		/// <param name="reason">The <see cref="Exception"/>.</param>
		/// <exception cref="Exception">Thrown if <paramref name="reason"/> is null.</exception>
		internal void Reject(Exception reason)
		{
			// validate arguments
			if (reason == null)
				throw new ArgumentNullException("reason");

			// make sure we are the only thread updating the state of this promise
			IEnumerable<RejectedCallback> callbacks;
			lock (stateSyncRoot)
			{
				// set the state to rejected
				state = PromiseState.Rejected;

				// store the reason
				rejectionReason = reason;

				// get the callbacks currently registered
				callbacks = rejectedCallbacks.ToArray();
				rejectedCallbacks.Clear();
			}

			// schedule all queued callbacks
			foreach (var callback in callbacks)
			{
				var callback1 = callback;
				scheduler.Schedule(() => callback1(reason));
			}
		}
		/// <summary>
		/// Registers 
		/// </summary>
		/// <param name="resolvedCallback">The <see cref="ResolvedCallback{TResult}"/> invoked if the promise is resolved.</param>
		/// <param name="rejectedCallback">The <see cref="RejectedCallback"/> invoked if the promise is rejected.</param>
		public void Then(ResolvedCallback<TValue> resolvedCallback, RejectedCallback rejectedCallback = null)
		{
			// validate arguments
			if (resolvedCallback == null)
				throw new ArgumentNullException("resolvedCallback");

			// enqueue the resolved callback
			Enqueue(resolvedCallback);

			// if there is a rejection handler, enqueue it
			if (rejectedCallback != null)
				Enqueue(rejectedCallback);
		}
		/// <summary>
		/// Enqueues the given <paramref name="callback"/>.
		/// </summary>
		/// <param name="callback">The callback which to enqueue.</param>
		private void Enqueue(ResolvedCallback<TValue> callback)
		{
			// make sure we are the only thread updating the state of this promise
			lock (stateSyncRoot)
			{
				// if the promise is unfulfilled, register the callback
				if (state == PromiseState.Unfulfilled)
					resolvedCallbacks.Enqueue(callback);
					// if the promise was already resolved, schedule the callback on the event loop
				else if (state == PromiseState.Resolved)
					scheduler.Schedule(() => callback(resolvedValue));
			}
		}
		/// <summary>
		/// Enqueues the given <paramref name="callback"/>.
		/// </summary>
		/// <param name="callback">The callback which to enqueue.</param>
		private void Enqueue(RejectedCallback callback)
		{
			// make sure we are the only thread updating the state of this promise
			lock (stateSyncRoot)
			{
				// if the promise is unfulfilled, register the callback
				if (state == PromiseState.Unfulfilled)
					rejectedCallbacks.Enqueue(callback);
					// if the promise was already rejected, schedule the callback on the event loop
				else if (state == PromiseState.Rejected)
					scheduler.Schedule(() => callback(rejectionReason));
			}
		}
	}
}