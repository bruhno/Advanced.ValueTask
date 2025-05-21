using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Channels;

using var cts = new CancellationTokenSource();

var channel = Channel.CreateBounded<long>(5);

var N = 1e7;


var completed = false;

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

var t2 = Task.Run(Read);
var t3 = Task.Run(Read);


async Task Read() 
{
    var reader = channel.Reader;
    var cnt = 0;

    while (true)
    {
        try
        {
            var i = await reader.ReadAsync(cts.Token);

            cnt++;

            if (i == N)
            {
                completed = true;
            }
        } catch (OperationCanceledException)
        {
            Console.WriteLine("Reading cancelled: "+cnt);
            return;
        }

        if (completed)
        {

            Console.WriteLine("Reading complete: "+cnt);
            cts.Cancel();
            return;
        }
    }

}


await Task.WhenAll(t1, t2, t3);

Console.WriteLine(sw.ElapsedMilliseconds);

Console.WriteLine("END");