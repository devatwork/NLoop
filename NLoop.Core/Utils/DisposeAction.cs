using System;

namespace NLoop.Core.Utils
{
	/// <summary>
	/// Executes the <see cref="callback"/> if disposed.
	/// </summary>
	public class DisposeAction : Disposable
	{
		/// <summary>
		/// The callback which to invoke if this object is disposed.
		/// </summary>
		private readonly Action callback;
		/// <summary>
		/// Creates a <see cref="DisposeAction"/>.
		/// </summary>
		/// <param name="callback">The callback which to invoke if this object is disposed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
		public DisposeAction(Action callback)
		{
			// validate arguments
			if (callback == null)
				throw new ArgumentNullException("callback");

			// store the callback
			this.callback = callback;
		}
		/// <summary>
		/// Dispose resources. Override this method in derived classes. Unmanaged resources should always be released
		/// when this method is called. Managed resources may only be disposed of if disposeManagedResources is true.
		/// </summary>
		/// <param name="disposeManagedResources">A value which indicates whether managed resources may be disposed of.</param>
		protected override void DisposeResources(bool disposeManagedResources)
		{
			// we only hold managed resources
			if (!disposeManagedResources)
				return;

			// invoke the callback
			callback();
		}
	}
}