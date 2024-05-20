using System.Runtime.CompilerServices;
using NetGL.experiment;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public static class packed_height {
    private const float min_height = -512.0f;
    private const float max_height = 512.0f;

    public static uint pack(float height, float3 normal) {
        // Normalize and scale the height to fit within an unsigned 16-bit integer range
        var normalized_height = (height - min_height) / (max_height - min_height);
        var packed_height     = (ushort)(normalized_height * ushort.MaxValue);

        // Normalize and scale the normal components from [-1, 1] to [0, 255]
        var nx = (byte)((normal.x * 0.5f + 0.5f) * 255.0f);
        var nz = (byte)((normal.z * 0.5f + 0.5f) * 255.0f);

        // Pack height and normals into a single 32-bit unsigned integer
        return (uint)((packed_height << 16) | (nx << 8) | nz);
    }

    public static (float height, float3 normal) unpack(uint packed) {
        // Extract the height
        var packed_height = (ushort)(packed >> 16);
        var height = (float)packed_height / ushort.MaxValue * (max_height - min_height) + min_height;

        // Extract normals
        var nx = (byte)((packed >> 8) & 0xFF);
        var nz = (byte)(packed & 0xFF);

        // Normalize normals back to [-1, 1]
        var normal_x = nx / 255.0f * 2.0f - 1.0f;
        var normal_z = nz / 255.0f * 2.0f - 1.0f;

        var sum_squares = normal_x * normal_x + normal_z * normal_z;
        var normal_y = sum_squares > 1.0f ? 0.0f : sqrt(1.0f - sum_squares) * float.Sign(height);

        var normal = normalize(float3(normal_x, normal_y, normal_z));

        return (height, normal);
    }
}

[SkipLocalsInit]
public class Heightmap: IDisposable {
    public Rectangle<int> area { get; } // in world space
    public Rectangle<int> texture_size { get; } // in pixels
    public Texture2D<float> texture { get; }

    public List<(float freq, float amp)> octaves { get; set; } = [
        (0.8f, 355f),
        (5.0f, 25f),
        (20.1f, 12f),
        (39.5f, 6.7f)
    ];

    public Heightmap(Rectangle<int> area, Rectangle<int> texture_size) {
        this.area = area;
        this.texture_size = texture_size;
        this.texture = new Texture2D<float>(texture_size.width, texture_size.height, PixelFormat.Red, PixelType.Float);
        this.texture.internal_pixel_format = PixelInternalFormat.R32f;
        this.texture.min_filter = TextureMinFilter.Linear;
        this.texture.mag_filter = TextureMagFilter.Linear;
        this.texture.wrap_s = TextureWrapMode.ClampToBorder;
        this.texture.wrap_t = TextureWrapMode.ClampToBorder;
        this.texture.create();
    }

    private void update_noise(Rectangle<int> area)
        => SimplexNoise2.generate_2d<SimplexKernel>(area, texture_size, texture.get_view(), octaves.ToArray(), 6);

    public void update() {
        update_noise(area);
        texture.update();
    }

    public void update_threaded() {
        var rects = texture_size.split(1, 8);
        Parallel.ForEach(rects, update_noise);
        texture.update();
    }

    public void Dispose() {
        texture.Dispose();
    }
}