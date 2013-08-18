using System;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Represents work that is not yet finished.
	/// </summary>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <typeparam name="TPromise">The type of <see cref="NLoop.Core.Promises.Promise{TValue}"/> returned by this deferred.</typeparam>
	public class Deferred<TValue, TPromise> where TPromise : Promise<TValue>
	{
		/// <summary>
		/// Constructs a new <see cref="Deferred{TValue,TPromise}"/>.
		/// </summary>
		/// <param name="promise">The <typeparamref name="TPromise"/> returned by this deferred.</param>
		internal Deferred(TPromise promise)
		{
			// validate arguments
			if (promise == null)
				throw new ArgumentNullException("promise");

			// create the promise
			Promise = promise;
		}
		/// <summary>
		/// Gets the <see cref="NLoop.Core.Promises.Promise{TValue}"/> for this deferred.
		/// </summary>
		public TPromise Promise { get; private set; }
		/// <summary>
		/// Resolves this <see cref="Deferred{TValue,TPromise}"/> with the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The <typeparamref name="TValue"/>.</param>
		/// <returns>Returns true if the state change was successful, otherwise false.</returns>
		public bool Resolve(TValue value)
		{
			return Promise.Resolve(value);
		}
		/// <summary>
		/// Rejects this <see cref="Deferred{TValue,TPromise}"/> with the given <paramref name="reason"/>.
		/// </summary>
		/// <param name="reason">The <see cref="Exception"/>.</param>
		/// <exception cref="Exception">Thrown if <paramref name="reason"/> is null.</exception>
		/// <returns>Returns true if the state change was successful, otherwise false.</returns>
		public bool Reject(Exception reason)
		{
			return Promise.Reject(reason);
		}
	}
}