namespace NetGL.Vectors;

/// <summary>
/// GL_INT_2_10_10_10_REV: A series of four values packed in a 32-bit unsigned integer.
/// Each individual packed value is a two's complement signed integer, but the overall
/// bitfield is unsigned. The bitdepth for the packed fields are 2, 10, 10, and 10, but
/// in reverse order.
/// </summary>
public readonly struct vec4_2_10_10_10 {
    private readonly uint value;

    public float r => unpack(this).x;
    public float g => unpack(this).y;
    public float b => unpack(this).z;
    public float a => unpack(this).w;

    public vec4_2_10_10_10(uint value)
        => this.value = value;

    public vec4_2_10_10_10(float r, float g, float b, float a)
        => value = pack(r, g, b, a);

    public static vec4_2_10_10_10 pack(float r, float g, float b, float a) {
        var R = (uint)(r * 1023f) & 0x3FF; // 10 bits for R
        var G = (uint)(g * 1023f) & 0x3FF; // 10 bits for G
        var B = (uint)(b * 1023f) & 0x3FF; // 10 bits for B
        var A = (uint)(a * 3f) & 0x3;      // 2 bits for A

        // Pack into a single UInt32
        return new((R << 20) | (G << 10) | B | (A << 30));
    }

    public static float4 unpack(vec4_2_10_10_10 packed) =>
        new(
            ((packed.value >> 20) & 0x3FF) / 1023f, // Extract R and normalize
            ((packed.value >> 10) & 0x3FF) / 1023f, // Extract G and normalize
            (packed.value & 0x3FF) / 1023f,         // Extract B and normalize
            ((packed.value >> 30) & 0x3) / 3f       // Extract A and normalize
           );

    public static implicit operator uint(vec4_2_10_10_10 packed) => packed.value;
    public static explicit operator vec4_2_10_10_10(uint packed) => new(packed);
    public static explicit operator float4(vec4_2_10_10_10 packed) => unpack(packed);

    public override string ToString() {
        var unpacked = unpack(this);
        return $"vec4_2_10_10_10({unpacked.x}, {unpacked.y}, {unpacked.z}, {unpacked.w})";
    }
}