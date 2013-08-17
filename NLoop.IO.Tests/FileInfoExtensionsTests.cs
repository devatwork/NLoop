using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NLoop.Core;
using NLoop.Core.Tests.Promises;
using NUnit.Framework;

namespace NLoop.IO.Tests
{
	[TestFixture]
	public class FileInfoExtensionsTests
	{
		[Test]
		public void ReadAllBytes()
		{
			// arrange
			var scheduler = new Mock<IScheduler>();
			scheduler.Setup(s => s.Schedule(It.IsAny<Action>())).Callback<Action>(action => Task.Run(action));
			var fileInfo = new FileInfo("NLoop.IO.Tests.dll");

			// assert parameter checking
			Assert.That(() => ((FileInfo) null).ReadAllBytes(scheduler.Object), Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => fileInfo.ReadAllBytes(null), Throws.InstanceOf<ArgumentNullException>());

			// check file not found
			var notExistingFile = new FileInfo("Idonotexist.file");
			var notExistingPromise = notExistingFile.ReadAllBytes(scheduler.Object);
			PromiseAssert.Rejects(notExistingPromise);

			// check file read
			var existingPromise = fileInfo.ReadAllBytes(scheduler.Object);
			PromiseAssert.Resolves(existingPromise);
		}
	}
}