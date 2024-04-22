using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NetGL;

internal class Garbage: IDisposable {
    private readonly long allocations_before;
    private readonly string name;
    private readonly Stopwatch stopwatch;

    private static readonly Dictionary<object, Garbage> instances = new();

    public static void start_measuring([CallerMemberName] string? caller = default) {
        if (instances.TryGetValue(string.Empty, out var instance)) {
            instance.Dispose();
            instances.Remove(string.Empty);
        }

        instances.Add(string.Empty, new Garbage(string.Intern(caller!)));
    }

    public static void start_measuring<T>(T caller) where T: class {
        if (instances.TryGetValue(caller, out var instance)) {
            instance.Dispose();
            instances.Remove(caller);
        }
        instances.Add(caller, new Garbage(string.Intern(caller.get_type_name())));
    }

    public static void stop_measuring([CallerMemberName] string? caller = default) {
        if (instances.TryGetValue(string.Empty, out var instance)) {
            instance.Dispose();
            instances.Remove(string.Empty);
            return;
        }

        Error.not_allocated(caller);
    }

    public static void stop_measuring<T>(T caller) where T: class {
        if (instances.TryGetValue(caller, out var instance)) {
            instance.Dispose();
            instances.Remove(caller);
            return;
        }

        Error.not_allocated(caller);
    }

    private Garbage([CallerMemberName] string? caller = default) {
        name = caller!;
        allocations_before = GC.GetAllocatedBytesForCurrentThread();
        stopwatch          = new();
        stopwatch.Start();
    }

    public void Dispose() {
        stopwatch.Stop();
        var allocations = GC.GetAllocatedBytesForCurrentThread() - allocations_before;
        Debug.println($"{name}: duration = {stopwatch.Elapsed.Milliseconds:N0} ms, allocated = {allocations:N0} bytes");
    }
}