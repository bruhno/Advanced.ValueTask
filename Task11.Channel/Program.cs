using System.Diagnostics;
using System.Threading.Channels;

using var cts = new CancellationTokenSource();

var channel = Channel.CreateBounded<long>(5);

var N = 1e7;

var sw = Stopwatch.StartNew();

var t1 = Task.Run(async () =>
{
    var writer = channel.Writer;

    long i = 0;

    while (i<=N)
    {        
        await writer.WriteAsync(i++, cts.Token);        
    }
    
    Console.WriteLine("Writing complete");
});

var t2 = Task.Run(async () =>
{
    var reader = channel.Reader;

    while (true)
    {
        var i = await reader.ReadAsync(cts.Token);

        if (i == N)
        {
            Console.WriteLine("Reading complete");
            return;
        }
    }

});

await Task.WhenAll(t1, t2);

Console.WriteLine(sw.ElapsedMilliseconds);

Console.WriteLine("END");