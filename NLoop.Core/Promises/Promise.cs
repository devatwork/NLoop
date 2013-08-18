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
		/// Holds the state of this promise, <see cref="PromiseStates"/> for candidates.
		/// </summary>
		private PromiseStates state = PromiseStates.Unfulfilled;
		/// <summary>
		/// Used for synchronizing <see cref="state"/> changes.
		/// </summary>
		private readonly object stateSyncRoot = new object();
		/// <summary>
		/// Holds the <typeparamref name="TValue"/> if this promise was <see cref="PromiseStates.Resolved"/>.
		/// </summary>
		private TValue resolvedValue;
		/// <summary>
		/// Holds the <see cref="Exception"/> if this promise was <see cref="PromiseStates.Rejected"/>.
		/// </summary>
		private Exception rejectionReason;
		/// <summary>
		/// Constructs a new <see cref="Promise{T}"/>.
		/// </summary>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to execute callbacks.</param>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
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
		/// <returns>Returns true if the state change was successful, otherwise false.</returns>
		internal bool Resolve(TValue value)
		{
			// try to resolve
			IEnumerable<ResolvedCallback<TValue>> callbacks = null;
			var changed = TryChangeState(PromiseStates.Resolved, () => {
				// store the result
				resolvedValue = value;

				// get the callbacks currently registered
				callbacks = resolvedCallbacks.ToArray();
				resolvedCallbacks.Clear();
			});
			if (!changed)
				return false;

			// schedule all queued callbacks
			foreach (var callback in callbacks)
			{
				var callback1 = callback;
				scheduler.Schedule(() => callback1(value));
			}
			return true;
		}
		/// <summary>
		/// Rejects this promise with the given <paramref name="reason"/>.
		/// </summary>
		/// <param name="reason">The <see cref="Exception"/>.</param>
		/// <returns>Returns true if the state change was successful, otherwise false.</returns>
		/// <exception cref="Exception">Thrown if <paramref name="reason"/> is null.</exception>
		internal bool Reject(Exception reason)
		{
			// validate arguments
			if (reason == null)
				throw new ArgumentNullException("reason");

			// try to reject
			IEnumerable<RejectedCallback> callbacks = null;
			var changed = TryChangeState(PromiseStates.Rejected, () => {
				// store the reason
				rejectionReason = reason;

				// get the callbacks currently registered
				callbacks = rejectedCallbacks.ToArray();
				rejectedCallbacks.Clear();
			});
			if (!changed)
				return false;

			// schedule all queued callbacks
			foreach (var callback in callbacks)
			{
				var callback1 = callback;
				scheduler.Schedule(() => callback1(reason));
			}
			return true;
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
		/// Try to change the state of the promise, will only be successful if the <see cref="state"/> equals <see cref="PromiseStates.Unfulfilled"/>.
		/// </summary>
		/// <param name="newState">The new <see cref="PromiseStates"/> which to try to set.</param>
		/// <param name="action">The <see cref="Action"/> which to execute if the state change is being made. This action is in a protected region, so no other threads can modify the state of this promise.</param>
		/// <returns>Returns true if the state change was successful, otherwise false.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="newState"/> equals <see cref="PromiseStates.Unfulfilled"/>.</exception>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		protected bool TryChangeState(PromiseStates newState, Action action)
		{
			// validate arguments
			if (newState == PromiseStates.Unfulfilled)
				throw new ArgumentException("Cannot change promise state to Unfulfilled", "newState");
			if (action == null)
				throw new ArgumentNullException("action");

			// check if the state is already set
			if (state != PromiseStates.Unfulfilled)
				return false;

			// make sure we are the only thread updating the state of this promise
			lock (stateSyncRoot)
			{
				// double check: if the state is already set
				if (state != PromiseStates.Unfulfilled)
					return false;

				// change the state
				state = newState;

				// execute the action
				action();
			}

			// return success
			return true;
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
				if (state == PromiseStates.Unfulfilled)
					resolvedCallbacks.Enqueue(callback);
					// if the promise was already resolved, schedule the callback on the event loop
				else if (state == PromiseStates.Resolved)
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
				if (state == PromiseStates.Unfulfilled)
					rejectedCallbacks.Enqueue(callback);
					// if the promise was already rejected, schedule the callback on the event loop
				else if (state == PromiseStates.Rejected)
					scheduler.Schedule(() => callback(rejectionReason));
			}
		}
	}
}