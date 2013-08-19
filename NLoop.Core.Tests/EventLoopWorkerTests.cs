using System;
using System.Threading;
using NUnit.Framework;

namespace NLoop.Core.Tests
{
	[TestFixture]
	public class EventLoopWorkerTests
	{
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
		public void ConstructorParameterChecking()
		{
			// assert
			Assert.That(() => new EventLoopWorker(null), Throws.InstanceOf<ArgumentNullException>());
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
		public void ThrowIfDisposed()
		{
			// arrange
			var nextCallback = new Func<Action>(() => null);
			var worker = new EventLoopWorker(nextCallback);

			// act
			worker.Start();
			worker.Dispose();

			// assert
			Assert.That(() => worker.IsRunning, Throws.InstanceOf<ObjectDisposedException>());
			Assert.That(() => worker.Start(), Throws.InstanceOf<ObjectDisposedException>());
		}
		[Test]
		public void WakeupAfterIdling()
		{
			// arrange
			var doWork = true;
			var counter = new CountdownEvent(2);
			var nextCallback = new Func<Action>(() => {
				if (!doWork)
					return null;

				doWork = false;
				return () => {
					Thread.Sleep(50);
					counter.Signal();
				};
			});
			var worker = new EventLoopWorker(nextCallback);

			// act
			worker.Start();
			Assert.That(worker.IsIdling, Is.False);

			// assert
			Thread.Sleep(100);
			Assert.That(worker.IsIdling, Is.True);

			// act
			doWork = true;

			// make sure the worker is waiting for the signal work
			Thread.Sleep(100);
			Assert.That(worker.IsIdling, Is.True);

			// act
			worker.SignalMoreWork();

			// assert that the work is actually done
			Assert.That(counter.Wait(100), Is.True);
		}
	}
}