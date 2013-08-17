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
	}
}