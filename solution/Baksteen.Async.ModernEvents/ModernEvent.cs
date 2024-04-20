﻿namespace Baksteen.Async.ModernEvents;

public sealed class ModernEvent<TEventArgs> : IModernEvent<TEventArgs>, IDisposable
{
    private class SubscriptionSlot(ModernEvent<TEventArgs> parent) : IDisposable
    {
        public Action<TEventArgs>? SyncSubscriber { get; init; }
        public Func<TEventArgs, Task>? AsyncSubscriber { get; init; }
        public void Dispose() => parent.Unsubscribe(this);
    }

    private readonly List<SubscriptionSlot> _subscriptions = [];
    private List<SubscriptionSlot> _cachedSubscriptions = [];
    private volatile bool _isStaleCache;

    public bool IgnoreSubscriberExceptions { get; init; } = true;

    public IDisposable Subscribe(Action<TEventArgs> action)
        => Subscribe(new SubscriptionSlot(this) { SyncSubscriber = action });

    public IDisposable Subscribe(Func<TEventArgs, Task> action)
        => Subscribe(new SubscriptionSlot(this) { AsyncSubscriber = action });

    private SubscriptionSlot Subscribe(SubscriptionSlot slot)
    {
        lock(_subscriptions)
        {
            _subscriptions.Add(slot);
            _isStaleCache = true;
            return slot;
        }
    }

    private void Unsubscribe(SubscriptionSlot slot)
    {
        lock(_subscriptions)
        {
            _isStaleCache = _subscriptions.Remove(slot);
        }
    }

    public async Task InvokeAsync(TEventArgs args)
    {
        if(_isStaleCache)
        {
            lock(_subscriptions)
            {
                if(_isStaleCache)
                {
                    _cachedSubscriptions = new(_subscriptions);
                    _isStaleCache = false;
                }
            }
        }

        foreach(var slot in _cachedSubscriptions)
        {
            if(slot.SyncSubscriber is not null)
            {
                if(IgnoreSubscriberExceptions)
                {
                    try { slot.SyncSubscriber(args); } catch { }
                }
                else
                {
                    slot.SyncSubscriber(args);
                }
            }

            if(slot.AsyncSubscriber is not null)
            {
                if(IgnoreSubscriberExceptions)
                {
                    try { await slot.AsyncSubscriber(args).ConfigureAwait(false); } catch { }
                }
                else
                {
                    await slot.AsyncSubscriber(args).ConfigureAwait(false);
                }
            }
        }
    }

    public void Dispose()
    {
        lock(_subscriptions)
        {
            _subscriptions.Clear();
            _cachedSubscriptions = [];
            _isStaleCache = true;
        }
    }
}
