using System;
using System.Linq;
using System.Threading.Tasks;

namespace Robust.UnitTesting
{
    public sealed class TestingParallelManager : IDisposable

    {
    public int Degree { get; }
    public TestingParallelManager(int degree) => Degree = Math.Max(1, degree);

    public void Run(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        Parallel.For(0, Degree, _ => action());
    }

    public void Run(Action<int> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        Parallel.For(0, Degree, action);
    }

    public Task RunAsync(Func<Task> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        return Task.WhenAll(Enumerable.Range(0, Degree).Select(_ => action()));
    }

    public void For(int count, Action<int> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        Parallel.For(0, count, action);
    }

    public void Dispose() { }
    }
}
