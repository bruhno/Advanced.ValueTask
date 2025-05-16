
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Sources;

//var valueTask = new MyValueTaskSource().RunAsync(10);


// Нельзя одновременно запускать await
// Внутри ManualResetValueTaskSourceCore есть проверка
// предотвращающая немедленную отправку continuation в тредпул
//await Task.WhenAll(

//    Task.Run(async () =>
//    {
//        var v = await valueTask;
//        Console.WriteLine($"T1: value {v}");
//    }),

//    Task.Run(async () =>
//    {
//        var v = await valueTask;
//        Console.WriteLine($"T2: value {v}");
//    })
//);


//var task = Task.Run(() =>
//{
//    var x = 15;
//    Console.WriteLine("calculate started: " + x);
//    Thread.Sleep(2000);
//    return x * 2;
//});


//// Двойной await для Task работает нормально
//// просто регистрируется два continuation
//await Task.WhenAll(

//    Task.Run(async () =>
//    {
//        var v = await task;
//        Console.WriteLine($"T1: value {v}");
//    }),

//    Task.Run(async () =>
//    {
//        var v = await task;
//        Console.WriteLine($"T2: value {v}");
//    })
//);


var arr = await Task.WhenAll(
    MyValueTaskSource.Run(10).AsTask(),
    MyValueTaskSource.Run(20).AsTask(),
    MyValueTaskSource.Run(30).AsTask()
    );

Console.WriteLine(string.Join(" ", arr));

class MyValueTaskSource: IValueTaskSource<int>
{

    public static ValueTask<int> Run(int x)
    {
        return new MyValueTaskSource().RunAsync(x);
    }



    public ValueTask<int> RunAsync(int x)
    {
        if (_cache.TryGetValue(x, out var cached))
        {
            Console.WriteLine("cache hit: "+x);
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
        Console.WriteLine($"on completed token: {token}");
        _core.OnCompleted(continuation, state, token, flags);
    }

    private static int CalculateData(int x)
    {
        Console.WriteLine("calculate started: "+x);
        Thread.Sleep(2000);
        return x * 2;
    }

    private ManualResetValueTaskSourceCore<int> _core = new();
    private static ConcurrentDictionary<int, int> _cache = new();
}