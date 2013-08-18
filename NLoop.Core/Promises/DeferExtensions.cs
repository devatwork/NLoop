using System;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Extension methods for creating <see cref="Deferred{TValue,TPromise}"/>s.
	/// </summary>
	public static class DeferExtensions
	{
		/// <summary>
		/// Creates a <see cref="Deferred{TValue,TPromise}"/> on the given <paramref name="scheduler"/>.
		/// </summary>
		/// <typeparam name="TValue">The type of value resolved by the <see cref="Deferred{TValue,TPromise}"/>.</typeparam>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to invoke the callbacks.</param>
		/// <returns>Returns the created <see cref="Deferred{TValue,TPromise}"/>.</returns>
		public static Deferred<TValue, Promise<TValue>> Defer<TValue>(this IScheduler scheduler)
		{
			// validate arguments
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			// create the promise
			var promise = new Promise<TValue>(scheduler);

			// return the created deferred
			return new Deferred<TValue, Promise<TValue>>(promise);
		}
		/// <summary>
		/// Creates a <see cref="Deferred{TValue,TPromise}"/> on the given <paramref name="scheduler"/>.
		/// </summary>
		/// <typeparam name="TValue">The type of value resolved by the <see cref="Deferred{TValue,TPromise}"/>.</typeparam>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to invoke the callbacks.</param>
		/// <param name="cancel">The action which to invoke if the <see cref="CancelablePromise{TValue}"/> is cancelled.</param>
		/// <returns>Returns the created <see cref="Deferred{TValue,TPromise}"/>.</returns>
		public static Deferred<TValue, CancelablePromise<TValue>> Defer<TValue>(this IScheduler scheduler, Action cancel)
		{
			// validate arguments
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");
			if (cancel == null)
				throw new ArgumentNullException("cancel");

			// create the promise
			var promise = new CancelablePromise<TValue>(scheduler, cancel);

			// return the created deferred
			return new Deferred<TValue, CancelablePromise<TValue>>(promise);
		}
	}
}