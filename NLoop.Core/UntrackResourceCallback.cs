using System.Threading;

namespace NLoop.Core
{
	/// <summary>
	/// Untracks a tracked resource.
	/// </summary>
	/// <returns>Returns true if the resource was still tracked, otherwise false.</returns>
	public delegate bool UntrackResourceCallback(CancellationToken token);
}