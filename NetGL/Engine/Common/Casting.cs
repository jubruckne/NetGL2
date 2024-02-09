using System.Runtime.InteropServices;

namespace NetGL;

public static class CastingExt {
    public static ref OUT reinterpret_cast<IN, OUT>(this ref IN input) where IN: struct where OUT: struct {
        return ref MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }

    public static OUT[] reinterpret_cast<IN, OUT>(this IN[] input) where IN: struct where OUT: struct {
        return MemoryMarshal.Cast<IN, OUT>(new Span<IN>(input)).ToArray();
    }
}