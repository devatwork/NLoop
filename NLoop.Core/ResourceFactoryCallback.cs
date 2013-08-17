using System;
using System.Threading;

namespace NLoop.Core
{
	/// <summary>
	/// Represents the method that creates a cancellable resource managed by an <see cref="EventLoop"/>.
	/// </summary>
	/// <param name="token">The <see cref="CancellationToken"/>.</param>
	/// <param name="unregister">Callback to unregister the resource from the <see cref="EventLoop"/>.</param>
	public delegate IDisposable ResourceFactoryCallback(CancellationToken token, Action unregister);
}