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
			Assert.That(loop.IsStarted, Is.True);
			Assert.That(() => wait.WaitOne(100), Is.True);
			Assert.That(counter, Is.EqualTo(3));
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
		public void StartWithCallbackParameterChecking()
		{
			Assert.That(() => new EventLoop().Start(null), Throws.InstanceOf<ArgumentNullException>());
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
				loop.Schedule(() => {
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
		[Test]
		public void TrackResourceCancel()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var cancelled = false;
			loop.TrackResource(token, untrack => {
				token.Register(() => { cancelled = true; });
				return new DisposeAction(() => { });
			});

			// act
			cts.Cancel();

			// assert
			Assert.That(cancelled, Is.True);
		}
		[Test]
		public void TrackResourceDispose()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var disposed = false;
			loop.TrackResource(token, untrack => new DisposeAction(() => { disposed = true; }));

			// act
			loop.Dispose();

			// assert
			Assert.That(disposed, Is.True);
		}
		[Test]
		public void TrackResourceParameterChecking()
		{
			// arrange
			var cts = new CancellationTokenSource();

			// assert
			Assert.That(() => new EventLoop().TrackResource(cts.Token, null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void TrackResourceUntrackedDisposed()
		{
			// arrange
			var cts = new CancellationTokenSource();
			var token = cts.Token;
			var loop = new EventLoop();
			var disposed = false;
			UnregisterResourceAction doUntrack = null;
			var disposer = new DisposeAction(() => { disposed = true; });
			loop.TrackResource(token, untrack => {
				doUntrack = untrack;
				return disposer;
			});

			// act
			IDisposable untrackedDisposer;
			var gotDisposer = doUntrack(out untrackedDisposer);
			untrackedDisposer.Dispose();

			// assert
			Assert.That(gotDisposer, Is.True, "Expected to get the disposer back");
			Assert.That(untrackedDisposer, Is.SameAs(disposer), "Expected the same disposer back");
			Assert.That(disposed, Is.True);
		}
	}
}