namespace  NetGL.Debug;

public static class Garbage {
    private static long TotalAllocatedBytes;

    public static void log() {
        Console.WriteLine($"TotalAllocatedBytes = {GC.GetTotalAllocatedBytes(true) - TotalAllocatedBytes:N0}");
        TotalAllocatedBytes = GC.GetTotalAllocatedBytes(true);
    }
}