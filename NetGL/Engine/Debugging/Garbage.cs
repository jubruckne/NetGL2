namespace NetGL.Debug;

using System.Runtime.CompilerServices;

internal static class Garbage {
    private static readonly ThreadLocal<long> allocations_before = new();

    public static void measure_begin() => allocations_before.Value = GC.GetAllocatedBytesForCurrentThread();

    public static void measure(string name) {
        var allocations = GC.GetAllocatedBytesForCurrentThread() - allocations_before.Value;
        Console.WriteLine($"{name}: {allocations:N0} bytes");
    }

    public static void measure(Action action, [CallerArgumentExpression("action")] string? name = default) {
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        var allocations = GC.GetAllocatedBytesForCurrentThread() - before;
        Console.WriteLine($"{name}: {allocations:N0}");
    }
}