namespace Baksteen.Async.ModernEvents;

public interface IModernEvent<TEventArgs>
{
    public IDisposable Subscribe(Action<TEventArgs> action);
    public IDisposable SubscribeAsync(Func<TEventArgs, Task> action);
}
