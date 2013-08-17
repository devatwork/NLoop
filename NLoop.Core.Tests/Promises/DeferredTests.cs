using System;
using Moq;
using NLoop.Core.Promises;
using NUnit.Framework;

namespace NLoop.Core.Tests.Promises
{
	[TestFixture]
	public class DeferredTests
	{
		[Test]
		public void ConstructorArgumentChecking()
		{
			// assert
			Assert.That(() => new Deferred<object>(null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void Constructor()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();

			// act
			var deferred = new Deferred<object>(scheduler.Object);

			// assert
			Assert.That(deferred.Promise, Is.Not.Null);
		}
		[Test]
		public void ResolveAfterAttachingHandler()
		{
			// arrange
			var result = new object();
			var callbackInvoked = false;
			var callback = new ResolvedCallback<object>(res => { callbackInvoked = true; });
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());

			// act
			var deferred = new Deferred<object>(scheduler.Object);
			deferred.Promise.Then(callback);
			deferred.Resolve(result);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
		[Test]
		public void ResolveBeforeAttachingHandler()
		{
			// arrange
			var result = new object();
			var callbackInvoked = false;
			var callback = new ResolvedCallback<object>(res => { callbackInvoked = true; });
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());

			// act
			var deferred = new Deferred<object>(scheduler.Object);
			deferred.Resolve(result);
			deferred.Promise.Then(callback);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
		[Test]
		public void RejectAfterAttachingHandler()
		{
			// arrange
			var result = new Exception();
			var callbackInvoked = false;
			var callback = new RejectedCallback(res => { callbackInvoked = true; });
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());

			// act
			var deferred = new Deferred<object>(scheduler.Object);
			deferred.Promise.Then(value => { }, callback);
			deferred.Reject(result);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
		[Test]
		public void RejectBeforeAttachingHandler()
		{
			// arrange
			var result = new Exception();
			var callbackInvoked = false;
			var callback = new RejectedCallback(res => { callbackInvoked = true; });
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());

			// act
			var deferred = new Deferred<object>(scheduler.Object);
			deferred.Reject(result);
			deferred.Promise.Then(value => { }, callback);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
	}
}