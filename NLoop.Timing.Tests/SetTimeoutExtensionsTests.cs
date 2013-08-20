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
		public void SetInterval()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var counter = new CountdownEvent(2);
			var callback = new Action(() => counter.Signal());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			loop.SetInterval(callback, timeout);

			// assert
			Assert.That(counter.Wait(timeout + timeout + timeout), Is.True);

			// cleanup
			loop.Dispose();
			counter.Dispose();
		}
		[Test]
		public void SetIntervalCancel()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callbackinvoked = new ManualResetEvent(false);
			var callback = new Action(() => callbackinvoked.Set());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			var timer = loop.SetInterval(callback, timeout);
			timer.Cancel();

			// assert
			Assert.That(callbackinvoked.WaitOne(timeout + timeout), Is.False);

			// cleanup
			loop.Dispose();
			callbackinvoked.Dispose();
		}
		[Test]
		public void SetIntervalParameterChecking()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callback = new Action(() => { });
			var timeout = TimeSpan.FromMilliseconds(100);

			// assert
			Assert.That(() => ((EventLoop) null).SetInterval(callback, timeout), Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => loop.SetInterval(null, timeout), Throws.InstanceOf<ArgumentNullException>());

			// cleanup
			loop.Dispose();
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

			// cleanup
			loop.Dispose();
			callbackinvoked.Dispose();
		}
		[Test]
		public void SetTimeoutCancel()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callbackinvoked = new ManualResetEvent(false);
			var callback = new Action(() => callbackinvoked.Set());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			var timer = loop.SetTimeout(callback, timeout);
			timer.Cancel();

			// assert
			Assert.That(callbackinvoked.WaitOne(timeout + timeout), Is.False);

			// cleanup
			loop.Dispose();
			callbackinvoked.Dispose();
		}
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

			// cleanup
			loop.Dispose();
		}
		[Test]
		public void SetTimeoutReset()
		{
			// arrange
			var loop = new EventLoop();
			loop.Start(() => { });
			var callbackinvoked = new ManualResetEvent(false);
			var callback = new Action(() => callbackinvoked.Set());
			var timeout = TimeSpan.FromMilliseconds(100);

			// act
			var cancelTimeout = loop.SetTimeout(callback, timeout);
			Thread.Sleep(50);
			cancelTimeout.Reset();
			Thread.Sleep(50);
			cancelTimeout.Reset();

			// assert
			Assert.That(callbackinvoked.WaitOne(0), Is.False);
			Assert.That(cancelTimeout, Is.Not.Null);
			Assert.That(callbackinvoked.WaitOne(timeout + timeout), Is.True);

			// cleanup
			loop.Dispose();
			callbackinvoked.Dispose();
		}
	}
}