using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

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
    public Texture2D<float> texture { get; }
    public readonly Noise.Settings noise_settings;

    public Heightmap(Rectangle<float> area, int texture_size) {
        noise_settings = new Noise.Settings(area, texture_size, [
            (1.38f, 380f),
            (5.0f, 25f),
            (10.1f, 15f),
            (30.5f, 8f)
        ]);

        this.texture = new Texture2D<float>(texture_size, texture_size, PixelFormat.Red, PixelType.Float);
        this.texture.internal_pixel_format = PixelInternalFormat.R32f;
        this.texture.min_filter = TextureMinFilter.Linear;
        this.texture.mag_filter = TextureMagFilter.Linear;
        this.texture.wrap_s = TextureWrapMode.ClampToBorder;
        this.texture.wrap_t = TextureWrapMode.ClampToBorder;
        this.texture.create();
    }

    private void update_noise() {
        Noise.generate_2d<SimplexKernel_SIMD, float>(noise_settings, texture.get_view(), 1);
/*
        Image img = ImageAsset.load_from_file(
                                              "3BF0C6AB-B626-491C-8B58-F48D115A1189_4_5005_c.jpeg",
                                              ColorComponents.Grey);

        for (int x = 0; x < texture.width; ++x) {
            for (int y = 0; y < texture.height; ++y) {
                var u = img.width * ((float)x / texture.width);
                var v = img.height * ((float)y / texture.height);

                var u0 = (int)Math.Floor(u);
                var v0 = (int)Math.Floor(v);

                var u1 = (u0 + 1).at_most(img.width - 1);
                var v1 = (v0 + 1).at_most(img.height - 1);

                float frac_u = u - u0;
                float frac_v = v - v0;

                float val11 = img.image_data[u0 + v0 * img.width];
                float val12 = img.image_data[u0 + v1 * img.width];
                float val21 = img.image_data[u1 + v0 * img.width];
                float val22 = img.image_data[u1 + v1 * img.width];

                float interp_val = (1 - frac_u) * (1 - frac_v) * val11 +
                                   (1 - frac_u) * frac_v * val12 +
                                   frac_u * (1 - frac_v) * val21 +
                                   frac_u * frac_v * val22;

                texture[x, y] *= (interp_val / 64f);
            }
        }
        */
    }

    public void update() {
        update_noise();
        texture.update();
    }

    public void Dispose() {
        texture.Dispose();
    }
}