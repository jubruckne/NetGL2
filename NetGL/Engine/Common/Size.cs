using System.Numerics;
using System.Runtime.InteropServices;

namespace NetGL;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Size {
    [FieldOffset(0)] public float width;
    [FieldOffset(4)] public float height;
    [FieldOffset(8)] public float depth;

    public Size(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public static Size unit_size = new Size(1f, 1f, 1f);

    public static implicit operator Size((float width, float height, float depth) size)
        => new Size(size.width, size.height, size.depth);

    public static implicit operator Vector3(in Size size) {
        return size.width.reinterpret<float, Vector3>();
    }
}