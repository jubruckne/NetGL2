namespace NetGL.Debug;

internal static class Garbage {
    private static readonly ThreadLocal<long> allocations_before = new();

    public static void measure_begin() => allocations_before.Value = GC.GetAllocatedBytesForCurrentThread();

    public static void measure(string name) {
        var allocations = GC.GetAllocatedBytesForCurrentThread() - allocations_before.Value;
        Console.WriteLine($"{Thread.CurrentThread.Name} {name}: allocated {allocations:N0} bytes");
    }
}