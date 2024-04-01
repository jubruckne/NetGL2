using System.Diagnostics;

namespace NetGL;

internal readonly ref struct Garbage {
    private readonly long allocations_before = new();
    private readonly string name;
    private readonly Stopwatch stopwatch;

    public Garbage() {
        name = "";
        allocations_before = GC.GetAllocatedBytesForCurrentThread();
        stopwatch          = new();
        stopwatch.Start();
    }

    public Garbage(string caller) {
        name = caller;
        allocations_before = GC.GetAllocatedBytesForCurrentThread();
        stopwatch          = new();
        stopwatch.Start();
    }

    public void Dispose() {
        stopwatch.Stop();
        var allocations = GC.GetAllocatedBytesForCurrentThread() - allocations_before;
        Debug.println($"{name}: duration={stopwatch.ElapsedMilliseconds:N0} ms, allocated={allocations:N0} bytes");
    }
}