using System;
using System.IO;
using NLoop.Core;
using NLoop.IO;
using NLoop.Timing;

namespace NLoop.TestApp
{
	public class Program
	{
		private static void Main(string[] args)
		{
			// create the event loop
			var loop = new EventLoop();

			// start it with a callback
			loop.Start(() => Console.WriteLine("Event loop has started"));

			// read the app.config
			var appConfigFile = new FileInfo("NLoop.TestApp.exe.config");
			var promise = appConfigFile.ReadAllBytes(loop);
			promise.Then(content => {
				Console.WriteLine("Success!! read {0} bytes from app.config", content.Length);
			}, reason => {
				Console.WriteLine("Dread!! got an error: {0}", reason);
			});

			// wait
			Console.WriteLine("Event loop is processing, press any key to exit");
			Console.Read();

			// we are done, dispose the loop so resources will be cleaned up
			loop.Dispose();
		}
	}
}