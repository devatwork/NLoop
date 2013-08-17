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

## Timing

The NLoop.Timing assembly provides timing related extension methods for the event loop.

### SetTimeout

Executes ```callback``` once after ```timeout```.

```csharp
// prints a message after 1 second
var cancel = loop.SetTimeout(() => Console.WriteLine("Something needs to happen right now!"), TimeSpan.FromSeconds(1));

// unless cancel is invoked within 1 second
cancel();
```

### SetInterval

Executes ```callback``` every ```timeout```.

```csharp
// prints a message every second
var cancel = loop.SetInterval(() => Console.WriteLine("I'm alive!"), TimeSpan.FromSeconds(1));

// stop after cancel is invoked
cancel();
```

## Copyright

Copyright © 2013 Bert Willems and contributors

## License

This project is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to LICENCE for more information.
