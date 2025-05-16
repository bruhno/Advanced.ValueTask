using System.Threading.Tasks.Sources;

var source = new ValueTaskCompletionSource<int>(true);

ValueTask<int> task1 = source.Task;
source.SetResult(42);

var task1ResultA = await task1;
Console.WriteLine($"Task1 {task1ResultA}");

var loopEnd = short.MaxValue * 2 + 1;

for (int i = 0; i <= loopEnd; i++)
{
    ValueTask<int> task2 = source.Task;
    source.SetResult(99);
    if (i == loopEnd)
    {
        break;
    }
    _ = await task2;
}

var task1ResultB = await task1;
Console.WriteLine($"Task1 {task1ResultB}");

public sealed class ValueTaskCompletionSource<TResult>(bool runContinuationsAsynchronously)
{
    public bool TrySetResult(TResult result) => _vts.TrySetResult(result, _vts.Version);

    public void SetResult(TResult result)
    {
        if (!TrySetResult(result))
        {
            throw new InvalidOperationException();
        }
    }

    public bool TrySetCanceled() => _vts.TrySetCanceled(_vts.Version);

    public void SetCanceled()
    {
        if (!TrySetCanceled())
        {
            throw new InvalidOperationException();
        }
    }

    public bool TrySetException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return _vts.TrySetException(ex, _vts.Version);
    }

    public void SetException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        if (!TrySetException(ex))
        {
            throw new InvalidOperationException();
        }
    }

    public ValueTask<TResult> Task => _vts.Task;
    private readonly ReusableValueTask<TResult> _vts = new(runContinuationsAsynchronously);
}


sealed class ReusableValueTask<TResult>: IValueTaskSource<TResult>, IValueTaskSource
{
    public ReusableValueTask(bool runContinuationsAsynchronously)
    {
        _mre.RunContinuationsAsynchronously = runContinuationsAsynchronously;
    }

    public TResult GetResult(short token)
    {
        lock (_tokenGuard)
        {
            try
            {
                var status = _mre.GetStatus(token);
                if (status == ValueTaskSourceStatus.Canceled)
                {
                    throw new TaskCanceledException();
                }

                return _mre.GetResult(token);
            }
            finally
            {
                _mre.Reset();
            }
        }
    }

    void IValueTaskSource.GetResult(short token) => GetResult(token);

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _mre.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        ArgumentNullException.ThrowIfNull(continuation);
        _mre.OnCompleted(continuation, state, token, flags);
    }

    public bool TrySetResult(TResult result, short token)
    {
        lock (_tokenGuard)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetResult(result);
                return true;
            }
            return false;
        }
    }

    public bool TrySetCanceled(short token)
        => TrySetException(new TaskCanceledException(), token);

    public bool TrySetException(Exception error, short token)
    {
        lock (_tokenGuard)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetException(error);
                return true;
            }

            return false;
        }
    }

    public ValueTask<TResult> Task => new(this, Version);
    public short Version => _mre.Version;
    private ManualResetValueTaskSourceCore<TResult> _mre;
    private readonly Lock _tokenGuard = new();
}