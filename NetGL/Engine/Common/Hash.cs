using System.Numerics;
using System.Runtime.CompilerServices;

namespace NetGL;

public static class Hash {
    private const int a = 1103515245;
    private const int c = 12345;
    private const int m = int.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int hash(this int i) {
        // linear congruential generator
        return Math.Abs(a * i + c) % m;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int hash<TInput>(this TInput s)
        where TInput: notnull {
        return s.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult hash<TInput, TResult>(this TInput s, TResult min, TResult max)
        where TResult: unmanaged, IBinaryInteger<TResult>
        where TInput: notnull {
        return s.GetHashCode().reinterpret<int, TResult>() % (max - min) + min;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult hash<TInput, TResult>(this TInput s, TResult min, TResult max, int seed)
        where TResult: unmanaged, IBinaryInteger<TResult>
        where TInput: notnull {

        var h = s.GetHashCode() ^ ((a * seed + c) % m);

        return h.reinterpret<int, TResult>() % (max - min) + min;
    }
}