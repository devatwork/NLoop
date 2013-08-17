using System;
using NLoop.Core;

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

			// do stuff here, for example schedule another callback
			loop.Schedule(() => Console.WriteLine("Another task"));

			Console.WriteLine("Event loop is processing, press any key to quit");
			Console.Read();

			// we are done, dispose the loop so resources will be cleaned up
			loop.Dispose();
		}
	}
}