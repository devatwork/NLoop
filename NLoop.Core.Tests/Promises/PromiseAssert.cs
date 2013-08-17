using System.Threading;
using NLoop.Core.Promises;
using NUnit.Framework;

namespace NLoop.Core.Tests.Promises
{
	/// <summary>
	/// Provide helpers to asser promises.
	/// </summary>
	public static class PromiseAssert
	{
		/// <summary>
		/// Asserts that the promise rejects.
		/// </summary>
		public static void Rejects<T>(Promise<T> promise)
		{
			// first check a promise is specified
			Assert.That(promise, Is.Not.Null, "The promise was null, expected an instance of the promise class.");

			// create a wait handle
			using (var wait = new ManualResetEventSlim(false))
			{
				// register the callbacks
				var rejected = false;
				promise.Then(value => wait.Set(), reason => {
					rejected = true;
					wait.Set();
				});

				// wait for timeout
				var isSet = wait.Wait(1000);
				Assert.That(isSet, Is.True, "The promise did not resolve or reject within the time limit");
				Assert.That(rejected, Is.True, "The promise was not rejected");
			}
		}
		/// <summary>
		/// Asserts that the promise resolves.
		/// </summary>
		public static void Resolves<T>(Promise<T> promise)
		{
			// first check a promise is specified
			Assert.That(promise, Is.Not.Null, "The promise was null, expected an instance of the promise class.");

			// create a wait handle
			using (var wait = new ManualResetEventSlim(false))
			{
				// register the callbacks
				var resolved = false;
				promise.Then(value => {
					resolved = true;
					wait.Set();
				}, reason => wait.Set());

				// wait for timeout
				var isSet = wait.Wait(1000);
				Assert.That(isSet, Is.True, "The promise did not resolve or reject within the time limit");
				Assert.That(resolved, Is.True, "The promise was not resolved");
			}
		}
	}
}