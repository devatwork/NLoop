using System;
using NLoop.Core;
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

			// prints a timeout after 1 second
			var cancel = loop.SetInterval(() => Console.WriteLine("Hello"), TimeSpan.FromSeconds(1));

			// unless cancel is invoked within 1 second
			cancel();

			Console.WriteLine("Event loop is processing, press any key to quit");
			Console.Read();

			// we are done, dispose the loop so resources will be cleaned up
			loop.Dispose();
		}
	}
}