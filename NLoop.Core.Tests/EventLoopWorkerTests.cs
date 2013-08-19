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
			// act
			Assert.That(() => new EventLoopWorker(), Throws.Nothing);
		}
		[Test, Repeat(10)]
		public void ProcessesCallbacks()
		{
			// arrange
			var worker = new EventLoopWorker();
			var workDone = new ManualResetEvent(false);
			workDone.Reset();

			// act
			worker.Schedule(() => workDone.Set());

			// assert
			Assert.That(workDone.WaitOne(TimeSpan.FromMilliseconds(100)), Is.True, "Exepcted work to be done within 100ms");

			// dispose stuff
			worker.Dispose();
			workDone.Dispose();
		}
		[Test]
		public void ThrowIfDisposed()
		{
			// arrange
			var worker = new EventLoopWorker();

			// act
			worker.Dispose();

			// assert
			Assert.That(() => worker.Schedule(() => { }), Throws.Nothing);
		}
	}
}