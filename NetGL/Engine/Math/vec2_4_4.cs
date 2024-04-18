namespace NetGL.Vectors;

public readonly struct vec2_4_4 {
    private readonly byte value;

    public byte x => unpack(this).x;
    public byte y => unpack(this).y;

    public vec2_4_4(byte value)
        => this.value = value;

    public vec2_4_4(byte x, byte y)
        => value = pack(x, y);

    public static vec2_4_4 pack(byte x, byte y)
        => new((byte)(((x & 0xF) << 4) | (y & 0xF)));

    public static byte2 unpack(vec2_4_4 packed) =>
        new((byte)((packed.value >> 4) & 0xF), (byte)(packed.value & 0xF));

    public static implicit operator byte(vec2_4_4 packed) => packed.value;
    public static implicit operator vec2_4_4((byte x, byte y) xy) => new(xy.x, xy.y);
    public static explicit operator byte2(vec2_4_4 packed) => unpack(packed);
    public static explicit operator vec2_4_4(byte packed) => new(packed);

    public override string ToString() {
        var unpacked = unpack(this);
        return $"vec2_4_4({unpacked.x}, {unpacked.y})";
    }
}