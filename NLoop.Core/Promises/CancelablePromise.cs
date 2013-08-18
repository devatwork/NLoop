using System;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Placeholder for a value which is might not be known initially but getting the value can be cancelled.
	/// </summary>
	/// <typeparam name="TValue">The type of value.</typeparam>
	public class CancelablePromise<TValue> : Promise<TValue>
	{
		/// <summary>
		/// Holds the <see cref="Action"/> which to invoke when this promise is cancelled.
		/// </summary>
		private readonly Action cancel;
		/// <summary>
		/// Constructs a new <see cref="CancelablePromise{T}"/>.
		/// </summary>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to execute callbacks.</param>
		/// <param name="cancel">The <see cref="Action"/> which to execute if the promise is cancelled.</param>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		internal CancelablePromise(IScheduler scheduler, Action cancel) : base(scheduler)
		{
			// validate arguments
			if (cancel == null)
				throw new ArgumentNullException("cancel");

			// store the arguments
			this.cancel = cancel;
		}
		/// <summary>
		/// Tries to cancel this promise. A promise can only be cancelled if its <see cref="Promise{TValue}.state"/> is <see cref="PromiseStates.Unfulfilled"/>.
		/// </summary>
		/// <returns>Returns true if the cancellation was succesfull, otherwise false.</returns>
		public bool Cancel()
		{
			// try to make the state change
			if (!TryChangeState(PromiseStates.Cancelled, () => { }))
				return false;

			// invoke the cancel action
			cancel();
			return true;
		}
	}
}