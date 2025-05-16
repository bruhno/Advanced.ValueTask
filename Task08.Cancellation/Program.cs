using System.Runtime.CompilerServices;

using var cts = new CancellationTokenSource();

await foreach (var item in GetB(200).WithCancellation(cts.Token))
{
    Console.WriteLine(item);

    if (item > 5) 
        cts.Cancel();
}

static async IAsyncEnumerable<int> GetB(int delay, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var i = 0;
    while (i < 10)
    {                
        await Task.Delay(delay,cancellationToken);

        yield return i++;
    }
}