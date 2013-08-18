using System;
using Moq;
using NLoop.Core.Promises;
using NUnit.Framework;

namespace NLoop.Core.Tests.Promises
{
	[TestFixture]
	public class DeferTests
	{
		[Test]
		public void DeferArgumentChecking()
		{
			// assert
			Assert.That(() => ((IScheduler) null).Defer<object>(), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void Defer()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();

			// act
			var deferred = scheduler.Object.Defer<object>();

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
			var deferred = scheduler.Object.Defer<object>();
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
			var deferred = scheduler.Object.Defer<object>();
			deferred.Resolve(result);
			deferred.Promise.Then(callback);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
		[Test]
		public void DoNoRejectAfterResolve()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());
			var resolveInvoked = false;
			var resolve = new ResolvedCallback<object>(val => { resolveInvoked = true; });
			var rejectInvoked = false;
			var reject = new RejectedCallback(reason => { rejectInvoked = true; });

			// act
			var deferred = scheduler.Object.Defer<object>();
			deferred.Promise.Then(resolve, reject);
			var resolved = deferred.Resolve(new object());
			var rejected = deferred.Reject(new Exception());

			// assert
			Assert.That(resolved, Is.True, "Expect the resolve call to be successful");
			Assert.That(resolveInvoked, Is.True, "Expect the resolve callback to be invoked");
			Assert.That(rejected, Is.False, "Did not expect the reject call to be successful");
			Assert.That(rejectInvoked, Is.False, "Did not expect the reject callback to be invoked");
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
			var deferred = scheduler.Object.Defer<object>();
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
			var deferred = scheduler.Object.Defer<object>();
			deferred.Reject(result);
			deferred.Promise.Then(value => { }, callback);

			// assert
			scheduler.VerifyAll();
			Assert.That(callbackInvoked, Is.True);
		}
		[Test]
		public void DoNoResolveAfterReject()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());
			var resolveInvoked = false;
			var resolve = new ResolvedCallback<object>(val => { resolveInvoked = true; });
			var rejectInvoked = false;
			var reject = new RejectedCallback(reason => { rejectInvoked = true; });

			// act
			var deferred = scheduler.Object.Defer<object>();
			deferred.Promise.Then(resolve, reject);
			var rejected = deferred.Reject(new Exception());
			var resolved = deferred.Resolve(new object());

			// assert
			Assert.That(rejected, Is.True, "Expected the reject call to be successful");
			Assert.That(rejectInvoked, Is.True, "Expected the reject callback to be invoked");
			Assert.That(resolved, Is.False, "Did not expect the resolve call to be successful");
			Assert.That(resolveInvoked, Is.False, "Did not expect the resolve callback to be invoked");
		}
		[Test]
		public void DeferWithCancelActionArgumentChecking()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			var cancel = new Action(() => { });

			// assert
			Assert.That(() => ((IScheduler) null).Defer<object>(cancel), Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => scheduler.Object.Defer<object>(null), Throws.InstanceOf<ArgumentNullException>());
		}
		[Test]
		public void DeferWithCancelAction()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());
			var cancelInvoked = false;
			var cancel = new Action(() => { cancelInvoked = true; });

			// act
			var deferred = scheduler.Object.Defer<object>(cancel);
			var cancelled = deferred.Promise.Cancel();

			// assert
			Assert.That(cancelled, Is.True, "Expected cancel to be successful");
			Assert.That(cancelInvoked, Is.True, "Expected cancel action to be invoked");
		}
		[Test]
		public void DoNotResolveOrRejectAfterCancel()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => action());
			var cancelInvoked = false;
			var cancel = new Action(() => { cancelInvoked = true; });
			var resolveInvoked = false;
			var resolve = new ResolvedCallback<object>(val => { resolveInvoked = true; });
			var rejectInvoked = false;
			var reject = new RejectedCallback(reason => { rejectInvoked = true; });

			// act
			var deferred = scheduler.Object.Defer<object>(cancel);
			deferred.Promise.Then(resolve, reject);
			var cancelled = deferred.Promise.Cancel();
			var resolved = deferred.Resolve(new object());
			var rejected = deferred.Reject(new Exception());

			// assert
			Assert.That(cancelled, Is.True, "Expected cancel to be successful");
			Assert.That(cancelInvoked, Is.True, "Expected cancel action to be invoked");
			Assert.That(resolved, Is.False, "Did not expect the resolve call to be successful");
			Assert.That(resolveInvoked, Is.False, "Did not expect the resolve callback to be invoked");
			Assert.That(rejected, Is.False, "Did not expect the reject call to be successful");
			Assert.That(rejectInvoked, Is.False, "Did not expect the reject callback to be invoked");
		}
	}
}