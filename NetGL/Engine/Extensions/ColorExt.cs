using OpenTK.Mathematics;

namespace NetGL;

public static class ColorExt {
    public static Color4 to_color(this (byte r, byte g, byte b) color) => new (color.r, color.g, color.b, byte.MaxValue);
}