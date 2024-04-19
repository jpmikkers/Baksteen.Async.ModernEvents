# Baksteen.Async.ModernEvents

ModernEvents are async capable events that support both sync and async subscribers. This implementation is modeled more or less after David Fowler's suggestion on twitter.

## Usage

On the producer side, we have to expose a `IModernEvent<TArg>` property:

```csharp
using Baksteen.Async.ModernEvents
...

public class ExampleWithModernEvents
{
    // example event argument record
    public record ClientAddedArgs(string Name);

    // privately declare a concrete ModernEvent field
    private ModernEvent<ClientAddedArgs> _clientAddedEvent = new();

    // publicly only expose the IModernEvent interface
    public IModernEvent<ClientAddedArgs> OnClientAdded { get => _clientAddedEvent; }

    public async Task AddClient(string newClientName)
    {
        // add client to database
        ...

        // notify all (sync & async) subscribers
        await _clientAddedEvent.InvokeAsync(new ClientAddedArgs{ Name = newClientName });
    }
}
```
\
On the consumer side, subscribing then looks as follows:

```csharp
var clientRegistry = new ExampleWithModernEvents();

// sync subscription:
clientRegistry.OnClientAdded.Subscribe(args =>
    Console.WriteLine($"Client {args.Name} was added"));

// async subscription:
clientRegistry.OnClientAdded.Subscribe(async args => {
    Console.WriteLine($"Client {args.Name} was added");
    await Task.CompletedTask; });
```
\
The `Subscribe()` method returns a `IDisposable` so the consumer can unsubscribe from the event:

```csharp
var clientRegistry = new ExampleWithModernEvents();

// async subscription:
var subscription = clientRegistry.OnClientAdded.Subscribe(async args => {
    Console.WriteLine($"Client {args.Name} was added");
    await Task.CompletedTask; });

// events will be received here

subscription.Dispose();

// we're no longer subscribed here
```
