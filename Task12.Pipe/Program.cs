using System;
using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Pipelines;

var N = 1e6;

long max = -1;

var sw = Stopwatch.StartNew();

var pipe = new Pipe();

var t1 = Task.Run(async () =>
{
    var i = 0;

    var writer = pipe.Writer;

    while (i <= N*10)
    {
        var span = writer.GetSpan(80);

        for (var j = 0; j < 10; j++)
        {
            BinaryPrimitives.TryWriteInt64LittleEndian(span.Slice(8*j), ++i);
            //Console.WriteLine("write:" + i);            
            writer.Advance(8);
            
        }



        var r = await writer.FlushAsync();

        if (r.IsCompleted)
        {
            break;
        }
    }

    max = i;

    //writer.Complete();
    Console.WriteLine("Writing complete");
});

var t2 = Task.Run(async () =>
{
    var reader = pipe.Reader;



    while (true)
    {
        var r = await reader.ReadAsync();

        if (r.IsCompleted)
        {
            break;
        }

        var buffer = r.Buffer;

        var list = ReadLong(buffer);

        foreach (var i in list)
        {
            if (i == max)
            {
                Console.WriteLine("Reading complete ");
                return;
            }

        }

        reader.AdvanceTo(buffer.GetPosition(8 * list.Count()));
    }    
});

static IEnumerable<long> ReadLong(in ReadOnlySequence<byte> buffer)
{
    var reader = new SequenceReader<byte>(buffer);

    var list = new List<long>();

    while (reader.Remaining >= 8)
    {
        if (reader.TryReadLittleEndian(out long value))
        {
            list.Add(value);
        }
        else
        {
            break;
        }
    }

    return list;
}

await Task.WhenAll(t1, t2);

Console.WriteLine(sw.ElapsedMilliseconds);
Console.WriteLine("END");