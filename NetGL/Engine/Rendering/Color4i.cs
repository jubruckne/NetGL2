using System.Security.Cryptography;
using OpenTK.Mathematics;

namespace NetGL;

public struct Color4i : IEquatable<Color4i> {
    private uint rgba;

    public Color4i() : this(uint.MaxValue) {}

    public Color4i(uint color) {
        rgba = color;
    }

    public Color4i(Color4 color) {
        rgba = color.to_int();
    }

    public Color4i(float r, float g, float b, float a = 1.0f) {
        rgba = (new Color4(r, g, b, a)).to_int();
    }

    public uint to_int() => rgba;

    public override string ToString() {
        return $"({r},{g},{b},{a})";
    }

    public byte r {
        get => (byte)(rgba & 0xFF);         // Extract the next 5 bits for Red
    }

    public byte g {
        get => (byte)((rgba >> 8) & 0xFF);  // Extract the next 5 bits for Green
    }

    public byte b {
        get => (byte)((rgba >> 16) & 0xFF); // Extract the next 5 bits for Green
    }

    public byte a {
        get => (byte)((rgba >> 24) & 0xFF); // Extract the next 5 bits for Green
    }

    public bool Equals(Color4i other) => rgba == other.rgba;
    public override bool Equals(object? obj) => obj is Color4i other && Equals(other);
    public override int GetHashCode() => (int)rgba;

    public static implicit operator Color4i(Color4 color) {
        return new Color4i(color);
    }

    public static implicit operator Color4i((float r, float g, float b) color) {
        return new Color4i(color.r, color.g, color.b, 1.0f);
    }

    public static implicit operator Color4i((float r, float g, float b, float a) color) {
        return new Color4i(color.r, color.g, color.b, color.a);
    }

    public static explicit operator Color4(Color4i color) {
        return new Color4(color.r, color.g, color.b, color.a);
    }

    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
        => (r, g, b, a) = (this.r, this.g, this.b, this.a);


    public static Color4i random_for(string name) {
        Random rnd = new(name.GetHashCode());
        float r = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        float g = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        float b = 0.2f + float.Pow(rnd.NextSingle() * 0.6f, 2);
        return new Color4i(r, g, b, 1.0f);
    }
}