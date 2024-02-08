using System.Runtime.InteropServices;

namespace NetGL;

public static class CastingExt {
    public static ref OUT reinterpret_cast<IN, OUT>(this ref IN input) where IN: unmanaged where OUT: unmanaged {
        return ref MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }
}