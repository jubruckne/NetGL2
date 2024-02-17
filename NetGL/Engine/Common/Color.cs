using System.Runtime.InteropServices;

namespace NetGL;

[StructLayout(LayoutKind.Explicit, Pack = 0)]
public struct Color {
    public static Color Black => new(0, 0, 0, 1);
    public static Color White => new(1, 1, 1, 1);

    public static Color PastelRed => new(1f, 0.4117647f, 0.4117647f, 1); // #FF6961
    public static Color PastelGreen => new(0.5960784f, 0.9843137f, 0.5960784f, 1); // #98FB98
    public static Color PastelBlue => new(0.6784314f, 0.8470588f, 0.9019608f, 1); // #AEC6CF
    public static Color PastelYellow => new(0.9921569f, 0.9215686f, 0.7058824f, 1); // #FDFD96
    public static Color PastelPurple => new(0.8705882f, 0.6196078f, 0.8392157f, 1); // #DEA5A4
    public static Color PastelOrange => new(1f, 0.7019608f, 0.4431373f, 1); // #FFB347

    [FieldOffset(0)] public System.Numerics.Vector4 vector4;
    [FieldOffset(0)] public System.Numerics.Vector3 vector3;

    [FieldOffset(0)] public float r;
    [FieldOffset(4)] public float g;
    [FieldOffset(8)] public float b;
    [FieldOffset(12)] public float a;

    public Color() => a = 1;
    public Color(float r, float g, float b, float a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public Color(float r, float g, float b) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 1.0f;
    }

    public Color(byte r, byte g, byte b) {
        this.r = r / 255f;
        this.g = g / 255f;
        this.b = b / 255f;
        this.a = 1f;
    }

    public static Color make(float r, float g, float b, float a) => new(r, g, b, a);
    public static Color make(float r, float g, float b) => new(r, g, b, 1f);
    public static Color make((float r, float g, float b) color) => new(color.r, color.g, color.b, 1);
    public static Color make((float r, float g, float b, float a) color) => new(color.r, color.g, color.b, color.a);
    public static Color make(byte r, byte g, byte b, byte a) => new(r / 256f, g / 256f, b / 256f, a / 256f);
    public static Color make(System.Numerics.Vector3 color) => new(color.X, color.Y, color.Z, 1f);
    public static Color make(System.Numerics.Vector4 color) => new(color.X, color.Y, color.Z, color.W);

    public static Color make(uint rgba) {
        float a = (rgba >> 24) & 0xFF;
        float b = (rgba >> 16) & 0xFF;
        float g = (rgba >> 8) & 0xFF;
        float r = rgba & 0xFF;

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public uint to_int() =>
        ((uint)(a * 255f) << 24) |
        ((uint)(b * 255f) << 16) |
        ((uint)(g * 255f) << 8) |
        (uint)(r * 255f);


    public static Color random_for<T>(in T obj) where T:notnull {
/*        Random rnd = new(obj.GetHashCode());
        float r = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        float g = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        float b = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        */

        var color = new Color(
            obj.hash<T, byte>(40, 135, 1445332),
            obj.hash<T, byte>(40, 135, 92432),
            obj.hash<T, byte>(40, 135, -4233)
            );

        return color;
    }

    public static implicit operator Color(System.Numerics.Vector3 color) => make(color);
    public static implicit operator Color(System.Numerics.Vector4 color) => make(color);

    public static implicit operator Color((float r, float g, float b) color) => make(color);
    public static implicit operator Color((float r, float g, float b, float a) color) => make(color);

    public override string ToString() => $"({r},{g},{b},{a})";

    // public static explicit operator System.Numerics.Vector3(Color color) => new(color.r, color.g, color.b);
    // public static explicit operator System.Numerics.Vector4(Color color) => new(color.r, color.g, color.b, color.a);

    // public static explicit operator OpenTK.Mathematics.Vector3(Color color) => color.vector_tk3;
    // public static explicit operator OpenTK.Mathematics.Vector4(Color color) => color.vector_tk4;
}