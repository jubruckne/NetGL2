using System.Numerics;
using System.Runtime.CompilerServices;

namespace NetGL.Vectors;

public readonly struct vec4_8_8_8_8<T>
    where T: unmanaged, INumber<T> {
    private readonly uint value;

    public T r => unpack(this).x;
    public T g => unpack(this).y;
    public T b => unpack(this).z;
    public T a => unpack(this).w;

    static vec4_8_8_8_8() {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(byte))
            throw new NotSupportedException();
    }

    private vec4_8_8_8_8(uint value) {
        this.value = value;
    }

    public vec4_8_8_8_8(T r, T g, T b, T a): this(pack(r, g, b, a)) {}

    public static vec4_8_8_8_8<T> pack(vec4<T> vector)
        => pack(vector.x, vector.y, vector.z, vector.w);

    public static vec4_8_8_8_8<T> pack(T r, T g, T b, T a) {
        if (typeof(T) == typeof(float)) {
            return new(
                       pack(
                            Unsafe.BitCast<T, float>(r),
                            Unsafe.BitCast<T, float>(g),
                            Unsafe.BitCast<T, float>(b),
                            Unsafe.BitCast<T, float>(a)
                           )
                      );
        }

        if (typeof(T) == typeof(byte)) {
            return new(
                       pack(
                            Unsafe.BitCast<T, byte>(r),
                            Unsafe.BitCast<T, byte>(g),
                            Unsafe.BitCast<T, byte>(b),
                            Unsafe.BitCast<T, byte>(a)
                           )
                      );
        }

        throw new NotSupportedException();
    }

    private static vec4_8_8_8_8<float> pack(float r, float g, float b, float a) {
        var R = (uint)(r * 255f) & 0xFF; // 8 bits for R
        var G = (uint)(g * 255f) & 0xFF; // 8 bits for G
        var B = (uint)(b * 255f) & 0xFF; // 8 bits for B
        var A = (uint)(a * 255f) & 0xFF; // 8 bits for A

        // Pack into a single UInt32
        return new((R << 24) | (G << 16) | (B << 8) | A);
    }

    private static vec4_8_8_8_8<byte> pack(byte r, byte g, byte b, byte a)
        => new ((uint)((r << 24) | (g << 16) | (b << 8) | a));

    public static vec4<T> unpack(vec4_8_8_8_8<T> packed) {
        if (typeof(T) == typeof(float)) {
            var v = unpack(new vec4_8_8_8_8<float>(packed.value));
            return Unsafe.BitCast<float4, vec4<T>>(v);
        }

        if (typeof(T) == typeof(byte)) {
            var v = unpack(new vec4_8_8_8_8<byte>(packed.value));
            return Unsafe.BitCast<byte4, vec4<T>>(v);
        }

        throw new NotSupportedException();
    }

    private static float4 unpack(vec4_8_8_8_8<float> packed) =>
        new(
            (packed.value >> 24) / 255f,          // Extract R and normalize
            ((packed.value >> 16) & 0xFF) / 255f, // Extract G and normalize
            ((packed.value >> 8) & 0xFF) / 255f,  // Extract B and normalize
            (packed.value & 0xFF) / 255f          // Extract A and normalize
           );

    private static byte4 unpack(vec4_8_8_8_8<byte> packed) =>
        new(
            (byte)(packed.value >> 24),          // Extract R and normalize
            (byte)((packed.value >> 16) & 0xFF), // Extract G and normalize
            (byte)((packed.value >> 8) & 0xFF),  // Extract B and normalize
            (byte)(packed.value & 0xFF)          // Extract A and normalize
           );

    public static implicit operator uint(vec4_8_8_8_8<T> packed) => packed.value;
    public static explicit operator vec4_8_8_8_8<T>(uint packed) => new(packed);
    public static explicit operator vec4_8_8_8_8<T>(int packed) => new((uint)packed);
}