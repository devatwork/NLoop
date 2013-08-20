using System;
using System.Threading;
using NLoop.Core.Utils;
using NUnit.Framework;

namespace NLoop.Core.Tests
{
	[TestFixture]
	public class EventLoopTests
	{
		[Test]
		public void Schedule()
		{
			// arrange
			var loop = new EventLoop();
			var wait = new AutoResetEvent(false);
			var counter = 0;

			// act
			loop.Schedule(() => counter++);
			loop.Schedule(() => counter++);
			loop.Start(() => wait.Set());
			loop.Schedule(() => counter++);

			// assert
			Assert.That(() => wait.WaitOne(100), Is.True);
			Assert.That(counter, Is.EqualTo(3));

			// cleanup
			loop.Dispose();
			wait.Dispose();
		}
		[Test]
		public void ScheduleParameterChecking()
		{
			Assert.That(() => new EventLoop().Schedule(null), Throws.InstanceOf<ArgumentNullException>());
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
			Assert.That(() => wait.WaitOne(100), Is.True);

			// cleanup
			loop.Dispose();
			wait.Dispose();
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
			Assert.That(() => wait.WaitHandle.WaitOne(100), Is.True);
			Assert.That(wait.CurrentCount, Is.EqualTo(0));

			// cleanup
			loop.Dispose();
			wait.Dispose();
		}
		[Test]
		public void StartWithCallbackParameterChecking()
		{
			Assert.That(() => new EventLoop().Start(null), Throws.InstanceOf<ArgumentNullException>());
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
		}
		[Test]
		public void TrackResourceCancel()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var cancelled = false;
			token.Register(() => { cancelled = true; });
			loop.TrackResource(token, new DisposeAction(() => { }));

			// act
			cts.Cancel();

			// assert
			Assert.That(cancelled, Is.True);

			// cleanup
			loop.Dispose();
			cts.Dispose();
		}
		[Test]
		public void TrackResourceDispose()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var disposed = false;
			loop.TrackResource(token, new DisposeAction(() => { disposed = true; }));

			// act
			loop.Dispose();

			// assert
			Assert.That(disposed, Is.True);

			// cleanup
			loop.Dispose();
			cts.Dispose();
		}
		[Test]
		public void TrackResourceParameterChecking()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			// assert
			Assert.That(() => new EventLoop().TrackResource(token, null), Throws.InstanceOf<ArgumentNullException>());

			// cleanup
			cts.Dispose();
		}
		[Test]
		public void TrackResourceUntrack()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var disposed = false;
			var disposer = new DisposeAction(() => { disposed = true; });
			var untrack = loop.TrackResource(token, disposer);

			// assert
			Assert.That(untrack(token), Is.True, "Expected the resource to be untracked");
			Assert.That(untrack(token), Is.False, "Expected the resource to be untracked already");

			// dispose the loop
			loop.Dispose();
			Assert.That(disposed, Is.False);

			// cleanup
			loop.Dispose();
			cts.Dispose();
		}
	}
}