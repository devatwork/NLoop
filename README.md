# NLoop

This library provides a high-performance managed event loop implementation for .NET, inspired by the Node.JS event loop model.

The callbacks executed on the event loop are all executed on the same thread, so you don't have to deal with threading issues.

## Getting started

```csharp
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
```

## Copyright

Copyright © 2013 Bert Willems and contributors

## License

This project is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to LICENCE for more information.
