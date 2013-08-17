using System;
using System.Threading;
using NLoop.Core;
using NUnit.Framework;

namespace NLoop.Timing.Tests
{
	[TestFixture]
	public class SetTimeoutExtensionsTests
	{
		[Test]
		public void SetTimeoutParameterChecking()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callback = new Action(() => { });
			var timeout = TimeSpan.FromMilliseconds(100);

			// assert
			Assert.That(() => ((EventLoop) null).SetTimeout(callback, timeout), Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => loop.SetTimeout(null, timeout), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void SetTimeout()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callbackinvoked = new ManualResetEvent(false);
			var callback = new Action(() => callbackinvoked.Set());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			var cancelTimeout = loop.SetTimeout(callback, timeout);

			// assert
			Assert.That(cancelTimeout, Is.Not.Null);
			Assert.That(callbackinvoked.WaitOne(timeout + timeout), Is.True);
		}
		[Test]
		public void CancelTimeout()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callbackinvoked = new ManualResetEvent(false);
			var callback = new Action(() => callbackinvoked.Set());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			var cancelTimeout = loop.SetTimeout(callback, timeout);
			cancelTimeout.Dispose();

			// assert
			Assert.That(callbackinvoked.WaitOne(timeout + timeout), Is.False);
		}
	}
}