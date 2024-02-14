using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL;

public static class CastingExt {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref OUT reinterpret_ref<IN, OUT>(this ref IN input) where IN: unmanaged where OUT: struct {
        return ref MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT reinterpret<IN, OUT>(this IN input) where IN: unmanaged where OUT: struct {
        return MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT[] reinterpret_ref<IN, OUT>(this IN[] input) where IN: unmanaged where OUT: struct {
        return MemoryMarshal.Cast<IN, OUT>(new Span<IN>(input)).ToArray();
    }
}