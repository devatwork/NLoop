namespace NLoop.Core.Promises
{
	/// <summary>
	/// Enumerates the different states a promise can be in.
	/// </summary>
	public enum PromiseStates
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
		Rejected = 2,
		/// <summary>
		/// The promise has been cancelled, meaning no value will be retrieved.
		/// </summary>
		Cancelled = 3
	}
}