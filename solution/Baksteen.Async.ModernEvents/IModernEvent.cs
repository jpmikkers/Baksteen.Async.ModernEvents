namespace Baksteen.Async.ModernEvents;

public interface IModernEvent<TEventArgs>
{
    public IDisposable Subscribe(Action<TEventArgs> action);
    public IDisposable Subscribe(Func<TEventArgs, Task> action);
}
