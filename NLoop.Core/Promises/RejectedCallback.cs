using System;

namespace NLoop.Core.Promises
{
	/// <summary>
	/// Callback invoked when a <see cref="Promise{T}"/> was rejected.
	/// </summary>
	/// <param name="reason">The <see cref="Exception"/> message explaining why the promise was rejected.</param>
	public delegate void RejectedCallback(Exception reason);
}