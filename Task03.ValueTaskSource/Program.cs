using System.Threading.Tasks.Sources;

//var v = new MySource();

//var nc = new MyAwaiter();

//await nc;

var t = Task.FromResult(15);

Console.WriteLine(t);

var v = new ValueTask();

//internal class MyAwaiter : INotifyCompletion
//{
//    public MyAwaiter GetAwaiter() => this;

//    public bool IsCompleted => false;

//    public void GetResult() => throw new NotImplementedException();

//    public void OnCompleted(Action continuation)
//    {
//        throw new NotImplementedException();
//    }
//}

internal struct MySource : IValueTaskSource
{

    public void GetResult(short token)
    {
        throw new NotImplementedException();
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return ValueTaskSourceStatus.Succeeded;
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        throw new NotImplementedException();
    }
}