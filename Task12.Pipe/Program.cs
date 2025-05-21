using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;


// Запись чтение long из PIPE порции long

var N = 10_000;
var BATCH = 10;
var READ_DELAY = TimeSpan.FromMilliseconds(2000);
var WRITE_DELAY = TimeSpan.FromMilliseconds(3000);
long max = -1;

var sw = Stopwatch.StartNew();

var pipe = new Pipe();

var t1 = Task.Run(async () =>
{
    long i = 0;

    var writer = pipe.Writer;

    while (i < N)
    {
        var span = writer.GetSpan(80);

        for (var j = 0; j < BATCH; j++)
        {
            BinaryPrimitives.TryWriteInt64LittleEndian(span.Slice(8 * j), ++i);
            writer.Advance(8);

        }

        if (i % 1000 == 0) await Task.Delay(WRITE_DELAY);

        var r = await writer.FlushAsync();

        if (r.IsCompleted)
        {
            break;
        }
    }

    max = i;

    Console.WriteLine("Writing complete");
});

var t2 = Task.Run(async () =>
{
    var reader = pipe.Reader;

    while (true)
    {
        var a = sw.ElapsedMilliseconds;

        var r = await reader.ReadAsync();

        var pause = sw.ElapsedMilliseconds - a;

        if (pause > 100)
        {
            Console.WriteLine($"read pause {pause} ms");
        }

        if (r.IsCompleted)
        {
            break;
        }

        var buffer = r.Buffer;

        var list = ReadLong(buffer);

        await Task.Delay(READ_DELAY);

        foreach (var i in list)
        {
            if (i % 1000 == 0)
                Console.WriteLine("Read " + i);

            if (i == max)
            {
                Console.WriteLine("Reading complete ");
                Console.WriteLine("max= " + i);
                return;
            }
        }

        var next = buffer.GetPosition(8 * list.Count());

        reader.AdvanceTo(next);
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