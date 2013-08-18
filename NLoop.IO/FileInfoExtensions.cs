using System;
using System.IO;
using System.Threading.Tasks;
using NLoop.Core;
using NLoop.Core.Promises;

namespace NLoop.IO
{
	/// <summary>
	/// Provides extensions to <see cref="FileInfo"/> to do evented I/O.
	/// </summary>
	public static class FileInfoExtensions
	{
		/// <summary>
		/// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
		/// </summary>
		/// <param name="fileInfo">The <see cref="FileInfo"/> from which to read all bytes.</param>
		/// <param name="scheduler">The <see cref="IScheduler"/> on which to execute the callbacks.</param>
		/// <returns>A <see cref="Promise{TValue}"/> which resolves to a byte array containing the contents of the file.</returns>
		/// <exception cref="ArgumentNullException">Thrown if one of the parameters is null.</exception>
		public static Promise<byte[]> ReadAllBytes(this FileInfo fileInfo, IScheduler scheduler)
		{
			// validate arguments
			if (fileInfo == null)
				throw new ArgumentNullException("fileInfo");
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			// create the deferred
			var deferred = scheduler.Defer<byte[]>();

			// open the file with the async flag
			Task.Run(() => {
				try
				{
					byte[] content;
					using (var fileStream = fileInfo.OpenRead())
					{
						var offset = 0;
						var length = fileStream.Length;
						var count = (int) length;
						content = new byte[count];
						while (count > 0)
						{
							var num = fileStream.Read(content, offset, count);
							offset += num;
							count -= num;
						}
					}

					// resolve the dererred with the read content
					deferred.Resolve(content);
				}
				catch (Exception exception)
				{
					// reject the deferred with the exception
					deferred.Reject(exception);
				}
			});

			// return the promise
			return deferred.Promise;
		}
	}
}