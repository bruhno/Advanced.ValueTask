using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks.Sources;


var s = new MyValueTaskSource();

for (int i = 1; i <= 20; i++)
{
    
    var v1 = await s.RunAsync(i);
    Console.WriteLine("VALUE: " + v1);

    var v2 = await s.RunAsync(i);
    Console.WriteLine("VALUE: " + v2);
}

class MyValueTaskSource: IValueTaskSource<int>
{
    public ValueTask<int> RunAsync(int x)
    {
        if (_cache.TryGetValue(x, out var cached))
        {
            return new ValueTask<int>(cached);
        }

        _core.Reset();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            var r = CalculateData(x);
            Console.WriteLine($"set result = {r} version: " + _core.Version);
            _cache.TryAdd(x, r);
            _core.SetResult(r);
        });

        return new ValueTask<int>(this, _core.Version);
    }

    public int GetResult(short token)
    {
        return _core.GetResult(token);
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        var status = _core.GetStatus(token);
        return status;
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted(continuation, state, token, flags);
    }

    private static int CalculateData(int x)
    {
        Thread.Sleep(200);
        return x * 2;
    }

    private ManualResetValueTaskSourceCore<int> _core = new();
    private static ConcurrentDictionary<int, int> _cache = new();
}
