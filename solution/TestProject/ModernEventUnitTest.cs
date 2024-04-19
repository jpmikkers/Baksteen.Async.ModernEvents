using Baksteen.Async.ModernEvents;

namespace TestProject;

[TestClass]
public class ModernEventUnitTest
{
    [TestMethod]
    public async Task Test_SubscribersInvokedSequentially()
    {
        List<int> callSequence = [];
        ModernEvent<int> modernEvent = new();

        modernEvent.Subscribe(x => callSequence.Add(1));
        modernEvent.Subscribe(async x => { callSequence.Add(2); await Task.CompletedTask; });
        modernEvent.Subscribe(x => callSequence.Add(3));
        modernEvent.Subscribe(async x => { callSequence.Add(4); await Task.CompletedTask; });

        await modernEvent.InvokeAsync(0);

        CollectionAssert.AreEquivalent(new List<int>([1, 2, 3, 4]), callSequence);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task Test_SubscriptionDispose()
    {
        List<int> callSequence = [];
        ModernEvent<int> modernEvent = new();

        var s1=modernEvent.Subscribe(x => callSequence.Add(1));
        var s2=modernEvent.Subscribe(async x => { callSequence.Add(2); await Task.CompletedTask; });
        modernEvent.Subscribe(x => callSequence.Add(3));
        modernEvent.Subscribe(async x => { callSequence.Add(4); await Task.CompletedTask; });

        await modernEvent.InvokeAsync(0);
        CollectionAssert.AreEquivalent(new List<int>([1, 2, 3, 4]), callSequence);

        callSequence.Clear();
        s1.Dispose();
        s2.Dispose();

        await modernEvent.InvokeAsync(0);
        CollectionAssert.AreEquivalent(new List<int>([3, 4]), callSequence);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task Test_EventDispose()
    {
        List<int> callSequence = [];
        ModernEvent<int> modernEvent = new();

        modernEvent.Subscribe(x => callSequence.Add(1));
        modernEvent.Subscribe(async x => { callSequence.Add(2); await Task.CompletedTask; });
        modernEvent.Subscribe(x => callSequence.Add(3));
        modernEvent.Subscribe(async x => { callSequence.Add(4); await Task.CompletedTask; });

        await modernEvent.InvokeAsync(0);
        CollectionAssert.AreEquivalent(new List<int>([1, 2, 3, 4]), callSequence);

        callSequence.Clear();
        modernEvent.Dispose();

        await modernEvent.InvokeAsync(0);
        CollectionAssert.AreEquivalent(new List<int>(), callSequence);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task Test_SubscriberExceptionsAreIgnoredByDefault()
    {
        List<int> callSequence = [];
        ModernEvent<int> modernEvent = new();

        using(modernEvent.Subscribe((Action<int>)(x => throw new Exception("blah"))))
        {
            await modernEvent.InvokeAsync(0);
        }

        using(modernEvent.Subscribe(x => throw new Exception("blah")))
        {
            await modernEvent.InvokeAsync(0);
        }
    }

    [TestMethod]
    public async Task Test_SubscriberExceptionsPropagated()
    {
        List<int> callSequence = [];
        ModernEvent<int> modernEvent = new() { IgnoreSubscriberExceptions = false };

        using(modernEvent.Subscribe((Action<int>)(x => throw new InvalidCastException("blah"))))
        {
            await Assert.ThrowsExceptionAsync<InvalidCastException>(async () => await modernEvent.InvokeAsync(0));
        }

        using(modernEvent.Subscribe(x => throw new InvalidOperationException("blah")))
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await modernEvent.InvokeAsync(0));
        }
    }
}
