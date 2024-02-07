using OpenTK.Mathematics;

namespace NetGL;

public static class Color4Ext {
    public static uint to_int(this in Color4 color) =>
        ((uint)(color.A * 255f) << 24) | 
        ((uint)(color.B * 255f) << 16) | 
        ((uint)(color.G * 255f) << 8) | 
        (uint)(color.R * 255f);

    public static ushort to_ushort(this in Color4 color) {
        ushort r = (ushort)(color.R * 31.0f);
        ushort g = (ushort)(color.G * 31.0f);
        ushort b = (ushort)(color.B * 31.0f);
        ushort a = (ushort)(color.A > 0.5f ? 1 : 0); // Alpha is 1 if greater than 0.5, else 0

        // Combine the ushort values
        return (ushort)((a << 15) | (r << 10) | (g << 5) | b);
    }

    public static byte to_byte(this in Color4 color) {
        byte r = (byte)(color.R * 3.0f);
        byte g = (byte)(color.G * 7.0f);
        byte b = (byte)(color.B * 7.0f);
        return (byte)((r << 6) | (g << 3) | b);
    }

    public static Color4 to_color4(this byte value, float alpha = 1f) {
        // Divide the byte into 3 parts (2 bits, 3 bits, 3 bits) for R, G, B
        float r = (value >> 6) & 0x03;  // Extract the top 2 bits
        float g = (value >> 3) & 0x07;  // Extract the next 3 bits
        float b = value & 0x07;         // Extract the bottom 3 bits

        // Normalize the values to the range [0, 1]
        // R has 4 levels, G and B have 8 levels
        r /= 3.0f;
        g /= 7.0f;
        b /= 7.0f;

        return new Color4(r, g, b, alpha);
    }

    public static Color4 to_color4(this uint rgba) {
        float a = (rgba >> 24) & 0xFF;
        float b = (rgba >> 16) & 0xFF;
        float g = (rgba >> 8) & 0xFF;
        float r = rgba & 0xFF;

        return new Color4(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color4 to_color4(this ushort rgba) {
        float a = (rgba >> 15) & 0x01; // Extract the top bit for Alpha
        float r = (rgba >> 10) & 0x1F; // Extract the next 5 bits for Red
        float g = (rgba >> 5) & 0x1F;  // Extract the next 5 bits for Green
        float b = rgba & 0x1F;         // Extract the bottom 5 bits for Blue

        // Normalize the values to the range [0, 1]
        r /= 31.0f;
        g /= 31.0f;
        b /= 31.0f;
        a = a > 0 ? 1.0f : 0.0f; // Alpha is either 0 or 1

        return new Color4(r, g, b, a);
    }
}