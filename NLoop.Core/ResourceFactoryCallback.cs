using System;

namespace NLoop.Core
{
	/// <summary>
	/// Represents the method that creates a cancellable resource managed by an <see cref="EventLoop"/>.
	/// </summary>
	/// <param name="unregister"><see cref="UnregisterResourceAction"/> to unregister the resource from the <see cref="EventLoop"/>.</param>
	public delegate IDisposable ResourceFactoryCallback(UnregisterResourceAction unregister);
}