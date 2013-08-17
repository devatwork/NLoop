using System;
using System.Threading;
using NUnit.Framework;

namespace NLoop.Core.Tests
{
	[TestFixture]
	public class EventLoopWorkerTests
	{
		[Test]
		public void ConstructorParameterChecking()
		{
			// assert
			Assert.That(() => new EventLoopWorker(null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void Constructor()
		{
			// arrange
			var nextCallback = new Func<Action>(() => null);

			// act
			var worker = new EventLoopWorker(nextCallback);

			// assert
			Assert.That(worker.IsRunning, Is.False);
		}
		[Test]
		public void Start()
		{
			// arrange
			var nextCallback = new Func<Action>(() => null);
			var worker = new EventLoopWorker(nextCallback);

			// act
			worker.Start();

			// assert
			Assert.That(worker.IsRunning, Is.True);
		}
		[Test]
		public void Stop()
		{
			// arrange
			var nextCallback = new Func<Action>(() => null);
			var worker = new EventLoopWorker(nextCallback);
			worker.Start();

			// act
			worker.Stop();

			// assert
			Assert.That(worker.IsRunning, Is.False);
		}
		[Test]
		public void IsProcessingCallbackQueue()
		{
			// arrange
			var workDone = new ManualResetEvent(false);
			var nextCallback = new Func<Action>(() => () => workDone.Set());
			var worker = new EventLoopWorker(nextCallback);
			Assert.That(workDone.WaitOne(100), Is.False);

			// act
			worker.Start();

			// assert
			Assert.That(worker.IsRunning, Is.True);
			Assert.That(workDone.WaitOne(100), Is.True);
		}
		[Test]
		public void DoesNotProcessesNullCallback()
		{
			// arrange
			var workDone = new ManualResetEvent(false);
			var nextCallback = new Func<Action>(() => {
				workDone.Set();
				return null;
			});
			var worker = new EventLoopWorker(nextCallback);
			Assert.That(workDone.WaitOne(100), Is.False);

			// act
			worker.Start();

			// assert
			Assert.That(worker.IsRunning, Is.True);
			Assert.That(workDone.WaitOne(100), Is.True);
		}
		[Test]
		public void ThrowIfDisposed()
		{
			// arrange
			var nextCallback = new Func<Action>(() => null);
			var worker = new EventLoopWorker(nextCallback);

			// act
			worker.Dispose();

			// assert
			Assert.That(() => worker.IsRunning, Throws.InstanceOf<ObjectDisposedException>());
			Assert.That(() => worker.Start(), Throws.InstanceOf<ObjectDisposedException>());
			Assert.That(() => worker.Stop(), Throws.InstanceOf<ObjectDisposedException>());
		}
	}
}