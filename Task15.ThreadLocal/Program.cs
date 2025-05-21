using var ThreadId = new ThreadLocal<long>(() => Environment.CurrentManagedThreadId, true);

Action action = () =>
{
    bool repeat = ThreadId.IsValueCreated;

    Console.WriteLine("ThreaId = {0} {1}", ThreadId.Value, repeat ? "(repeat)" : "");    
};


Parallel.Invoke(action, action, action, action, action, action, action, action,
    action, action, action, action, action, action, action, action,
    action, action, action, action, action, action, action, action,
    action, action, action, action, action, action, action, action);


foreach (var id in ThreadId.Values)
{
    Console.WriteLine("value: "+id);
}