
var t = GetIntAsync();

Console.WriteLine(t.Status);

Task<bool> GetBoolAsync()
{
    return Task.FromResult(true);
}

Task<int> GetIntAsync()
{
    return Task.FromResult(5);
}