using System;
using System.IO;
using System.Net.Http;
using NLoop.Core;
using NLoop.IO;
using NLoop.Net.Http;
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

			// send a web request
			var httpClient = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/");
			var httpPromise = httpClient.Send(loop, request);
			httpPromise.Then(content => {
				Console.WriteLine("Success!! read {0} from google.com", content.StatusCode);
			}, reason => {
				Console.WriteLine("Dread!! got an error: {0}", reason);
			});
			//httpPromise.Cancel();

			// wait
			Console.WriteLine("Event loop is processing, press any key to exit");
			Console.Read();

			// we are done, dispose the loop so resources will be cleaned up
			loop.Dispose();
		}
	}
}