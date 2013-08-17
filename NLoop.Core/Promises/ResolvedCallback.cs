namespace NLoop.Core.Promises
{
	/// <summary>
	/// Callback invoked when a <see cref="Promise{T}"/> was resolved.
	/// </summary>
	/// <typeparam name="TValue">The typeof value.</typeparam>
	/// <param name="value">The resolved value.</param>
	public delegate void ResolvedCallback<in TValue>(TValue value);
}