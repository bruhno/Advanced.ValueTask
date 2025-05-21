using DotNext.Threading;

var i = 1;

var val = new AsyncLazy<int>(t => Task.Run(async () =>
{
    await Task.Delay(1000);
    return ++i;
}, t),
resettable: true
);

_ = Task.Run(async () =>
{
    await Task.Delay(100);

    Console.WriteLine(await val.WithCancellation(CancellationToken.None));
});


_ = Task.Run(async () =>
{
    await Task.Delay(200);

    Console.WriteLine(await val.WithCancellation(CancellationToken.None));
});

await Task.Delay(5000);

val.Reset();

Console.WriteLine(await val.WithCancellation(CancellationToken.None));