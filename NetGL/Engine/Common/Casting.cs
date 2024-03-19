using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL;

public static class CastingExt {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT convert<OUT>(this in Vector3 input) where OUT: unmanaged => convert<Vector3, OUT>(input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT convert<OUT>(this in Vector3h input) where OUT: unmanaged => convert<Vector3h, OUT>(input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT convert<IN, OUT>(this IN input) where IN: unmanaged where OUT: unmanaged {
        if (typeof(IN) == typeof(OUT))
            return Unsafe.BitCast<IN, OUT>(input);

        if (can_reinterpret<IN, OUT>())
            return input.reinterpret<IN, OUT>();

        if (typeof(IN) == typeof(Vector3) && typeof(OUT) == typeof(Vector3h))
            return new Vector3h(input.reinterpret<IN, Vector3>()).reinterpret<Vector3h, OUT>();

        if (typeof(IN) == typeof(Vector3h) && typeof(OUT) == typeof(Vector3))
            return new Vector3(input.reinterpret<IN, Vector3h>()).reinterpret<Vector3, OUT>();

        dynamic dyn = input;
        return dyn;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool is_vector3<T>() =>
        typeof(T) == typeof(Vector3)
        || typeof(T) == typeof(System.Numerics.Vector3)
        || typeof(T) == typeof((float, float, float));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool can_reinterpret<IN, OUT>() where IN: struct where OUT: struct {
        return is_vector3<IN>() || is_vector3<OUT>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref OUT reinterpret_ref<IN, OUT>(this ref IN input) where IN: unmanaged where OUT: unmanaged {
        return ref MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT reinterpret<IN, OUT>(this IN input) where IN: unmanaged where OUT: struct {
        return MemoryMarshal.Cast<IN, OUT>(MemoryMarshal.CreateSpan(ref input, 1))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OUT[] reinterpret_ref<IN, OUT>(this IN[] input) where IN: unmanaged where OUT: unmanaged {
        return MemoryMarshal.Cast<IN, OUT>(new Span<IN>(input)).ToArray();
    }
}