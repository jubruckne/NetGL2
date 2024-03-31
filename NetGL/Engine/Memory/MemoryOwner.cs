using System.IO.MemoryMappedFiles;

namespace NetGL;

public unsafe class MemoryOwner {
    public MemoryOwner() {
        var mmf = MemoryMappedFile.CreateNew(null, 100000);
        var mv  = mmf.CreateViewAccessor();

        byte* ptr = null;
        mv.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

        mv.SafeMemoryMappedViewHandle.ReleasePointer();
    }
}