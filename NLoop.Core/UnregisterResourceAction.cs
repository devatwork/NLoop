using System;

namespace NLoop.Core
{
	/// <summary>
	/// Represents a method that unregisters a resource from an <see cref="EventLoop"/>.
	/// </summary>
	/// <param name="disposer">The <see cref="IDisposable"/> registered as the cleanup action.</param>
	/// <returns>Returns true if the resource was still registered with the <see cref="EventLoop"/>.</returns>
	public delegate bool UnregisterResourceAction(out IDisposable disposer);
}