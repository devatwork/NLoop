using System;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Represents work that is not yet finished.
	/// </summary>
	/// <typeparam name="TValue">The type of value.</typeparam>
	public class Deferred<TValue>
	{
		/// <summary>
		/// Gets the <see cref="NLoop.Core.Promises.Promise{TValue}"/> for this deferred.
		/// </summary>
		public Promise<TValue> Promise { get; private set; }
		/// <summary>
		/// Constructs a new <see cref="Deferred{T}"/>.
		/// </summary>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to execute callbacks.</param>
		public Deferred(IScheduler scheduler)
		{
			// validate arguments
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			// create the promise
			Promise = new Promise<TValue>(scheduler);
		}
		/// <summary>
		/// Resolves this <see cref="Deferred{TValue}"/> with the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The <typeparamref name="TValue"/>.</param>
		public void Resolve(TValue value)
		{
			Promise.Resolve(value);
		}
		/// <summary>
		/// Rejects this <see cref="Deferred{TValue}"/> with the given <paramref name="reason"/>.
		/// </summary>
		/// <param name="reason">The <see cref="Exception"/>.</param>
		/// <exception cref="Exception">Thrown if <paramref name="reason"/> is null.</exception>
		public void Reject(Exception reason)
		{
			Promise.Reject(reason);
		}
	}
}