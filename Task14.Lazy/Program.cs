
using System.Diagnostics;

var i = 0;

var sw = Stopwatch.StartNew();

// LazyThreadSafetyMode.PublicationOnly : The result is 2
// LazyThreadSafetyMode.ExecutionAndPublication : The result is 1

var val = new Lazy<int>(() =>
{
    i++;

    Thread.Sleep(i==1?1000:100); // the first call is longer

    return i;
},
LazyThreadSafetyMode.PublicationOnly 
);

_ = Task.Run(async () =>
{
    await Task.Delay(100);
    Console.WriteLine(val.Value);
});

_ = Task.Run(async () =>
{
    await Task.Delay(300);
    Console.WriteLine(val.Value);
});

await Task.Delay(5000);