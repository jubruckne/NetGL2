using System.Numerics;
using System.Runtime.CompilerServices;

namespace NetGL;

[SkipLocalsInit]
public class Xorshift {
    public static Xorshift shared => _shared.Value!;
    private static readonly ThreadLocal<Xorshift> _shared = new();
    private ulong state = 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong next() {
        state ^= state >> 12;
        state ^= state << 25;
        state ^= state >> 27;
        return state * 0x2545F4914F6CDD1DUL;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T next<T>() where T: unmanaged, INumberBase<T> {
        var result = T.Zero;
        var r = next();

        switch (result) {
            case float or double or half: return T.CreateTruncating(r / (double)ulong.MaxValue);
            case ulong or long:           return T.CreateTruncating(r);
            case int or uint:             return T.CreateTruncating((uint)(r << 32) ^ (uint)(r >> 32));
            case short or ushort:         return T.CreateTruncating((ushort)(((r >> 47) ^ (r >> 23)) & 0xFFFF));
            case byte:                    return T.CreateTruncating((byte)((r >> 56) ^ r));
            default:                      return Error.type_conversion_error<T, T>(result);
        }
    }

    public void fill<T>(Span<T> data) where T: unmanaged, INumberBase<T> {
        for (var i = 0; i < data.Length; ++i)
            data[i] = next<T>();
    }

    public void fill(Span<bool> data) {
        for (var i = 0; i < data.Length; ++i)
            data[i] = (next<int>() & 0x1) == 0;
    }
}