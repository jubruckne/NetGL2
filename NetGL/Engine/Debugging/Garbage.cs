namespace  NetGL.Debug;

public static class Garbage {
    private static long TotalAllocatedBytes;

    public static void log() {
        Console.WriteLine($"TotalAllocatedBytes = {GC.GetTotalAllocatedBytes() - TotalAllocatedBytes}");
        TotalAllocatedBytes = GC.GetTotalAllocatedBytes();
    }
}