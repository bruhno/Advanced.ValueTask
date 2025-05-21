var i = new ThreadLocal<int>(() => 0);
var j = new AsyncLocal<int>();


Console.WriteLine("i:"+ ++i.Value);
Console.WriteLine("j:"+ ++j.Value);


for (var k = 0; k < 10; k++) {

    await Task.Yield();

    Console.WriteLine("yield "+Environment.CurrentManagedThreadId);

    Console.WriteLine("i:" + ++i.Value);
    Console.WriteLine("j:" + ++j.Value);


}

ThreadPool.QueueUserWorkItem(_ =>
{
    Console.WriteLine("queue " + Environment.CurrentManagedThreadId);

    Console.WriteLine("i:" + ++i.Value);
    Console.WriteLine("j:" + ++j.Value);
});

Thread.Sleep(500);

ThreadPool.UnsafeQueueUserWorkItem(_ =>
{
    Console.WriteLine("unsafe " + Environment.CurrentManagedThreadId);

    Console.WriteLine("i:" + ++i.Value);
    Console.WriteLine("j:" + ++j.Value);
}, null);