using System;
using System.Threading;
using NUnit.Framework;

namespace NLoop.Core.Tests
{
	[TestFixture]
	public class EventLoopTests
	{
		[Test]
		public void StartWithCallbackParameterChecking()
		{
			Assert.That(() => new EventLoop().Start(null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void StartWithCallback()
		{
			// arrange
			var loop = new EventLoop();
			var wait = new AutoResetEvent(false);

			// act
			loop.Start(() => wait.Set());

			// assert
			Assert.That(loop.IsStarted, Is.True);
			Assert.That(() => wait.WaitOne(100), Is.True);
		}
		[Test]
		public void StartWithCallbackCalledTwice()
		{
			// arrange
			var loop = new EventLoop();
			var wait = new CountdownEvent(2);

			// act
			loop.Start(() => wait.Signal());
			loop.Start(() => wait.Signal());

			// assert
			Assert.That(loop.IsStarted, Is.True);
			Assert.That(() => wait.WaitHandle.WaitOne(100), Is.True);
			Assert.That(wait.CurrentCount, Is.EqualTo(0));
		}
		[Test]
		public void AddParameterChecking()
		{
			Assert.That(() => new EventLoop().Add(null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void Add()
		{
			// arrange
			var loop = new EventLoop();
			var wait = new AutoResetEvent(false);
			var counter = 0;

			// act
			loop.Add(() => counter++);
			loop.Add(() => counter++);
			loop.Start(() => wait.Set());
			loop.Add(() => counter++);

			// assert
			Assert.That(loop.IsStarted, Is.True);
			Assert.That(() => wait.WaitOne(100), Is.True);
			Assert.That(counter, Is.EqualTo(3));
		}
		[Test]
		public void Stop()
		{
			// arrange
			var loop = new EventLoop();
			var wait = new AutoResetEvent(false);
			const int loops = 10;
			const int sleep = 10;
			const int maxSleep = (loops + 1)*sleep;
			for (var index = 0; index < loops; index++)
			{
				var localIndex = index;
				loop.Add(() => {
					Thread.Sleep(sleep);
					if (localIndex == loops - 1)
						wait.Set();
				});
			}

			// act
			loop.Start(() => { });
			loop.Stop();

			// assert
			Assert.That(() => wait.WaitOne(maxSleep), Is.False);
		}
		[Test]
		public void ThrowIfDisposed()
		{
			// arrange
			var loop = new EventLoop();

			// act
			loop.Start(() => { });
			loop.Dispose();

			// assert
			Assert.That(() => loop.Start(() => { }), Throws.InstanceOf<ObjectDisposedException>());
			Assert.That(() => loop.Stop(), Throws.InstanceOf<ObjectDisposedException>());
		}
	}
}