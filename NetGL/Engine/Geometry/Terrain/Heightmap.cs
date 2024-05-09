using System.Runtime.CompilerServices;
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
    public Rectangle bounds { get; } // in world space
    public int texture_size { get; } // in pixels
    public Texture2D<float> texture { get; }

    public Heightmap(int texture_size, Rectangle bounds) {
        this.bounds = bounds;
        this.texture_size = texture_size;
        this.texture = new Texture2D<float>(texture_size, texture_size, PixelFormat.Red, PixelType.Float);
        this.texture.internal_pixel_format = PixelInternalFormat.R32f;
        this.texture.min_filter = TextureMinFilter.Linear;
        this.texture.mag_filter = TextureMagFilter.Linear;
        this.texture.wrap_s = TextureWrapMode.ClampToBorder;
        this.texture.wrap_t = TextureWrapMode.ClampToBorder;
        this.texture.create();
    }

    private void generate(TerrainNoise noise, Rectangle<int> area) {
        for (var x = area.left; x < area.right; ++x) {
            var px = bounds.left + bounds.width * x / texture_size;

            for (var y = area.bottom; y < area.top; ++y) {
                var height = noise.sample(
                                          px,
                                          bounds.bottom + bounds.height * y / texture_size
                                         );

                /*var normal = normalize(
                                       float3(
                                              bounds.left + bounds.width * x / texture_size,
                                              height,
                                              bounds.bottom + bounds.height * y / texture_size
                                             )
                                      );
*/
                texture[x, y] = height;
            }
        }
    }

    public void generate_threaded(TerrainNoise noise) {
        var rects = Rectangle<int>.square(0, 0, texture_size).split(1, 8);

        Parallel.ForEach(
                         rects,
                         (rect) =>
                             generate(noise, rect)
                        );

        texture.update();

    }

/*
        for (var i = 0; i < texture_size * texture_size; ++i) {
            var px = i % texture_size;
            var py = i / texture_size;

            // sample noise at pixel position

            var height =
                noise.sample(
                             bounds.left + bounds.width * px / texture_size,
                             bounds.bottom + bounds.height * py / texture_size
                            );

            var normal = normalize(
                                   float3(
                                          bounds.left + bounds.width * px / texture_size,
                                          height,
                                          bounds.bottom + bounds.height * py / texture_size
                                         )
                                  );

            //var hn = packed_height.pack(height, normal);
            //var hn2 = packed_height.unpack(hn);
            //Console.WriteLine($"original={(height, normal)}, unpacked={hn2}");

            // pack height into 2x 8-bit channels
            texture[px, py] = height;
        }
*/


    public void generate(TerrainNoise noise) {
        /*FastNoise2 fractal = new FastNoise2("FractalFBm");
        fractal.Set("Source", new FastNoise2("Simplex"));
        fractal.Set("Gain", new FastNoise2("Simplex"));
        fractal.Set("Lacunarity", 0.7f);
        fractal.Set("Octaves", 5);

        fractal.GenUniformGrid2D(
                                 texture.as_span(),
                                 0,
                                 0,
                                 texture_size,
                                 texture_size,
                                 0.02f,
                                 3838
                                );

        Debug.println(texture.as_span().random_sample(), ConsoleColor.Green);
        texture.update();

        return;*/

        List<(float, float)> octaves = [
            (1.1f, 555f),
            (10.0f, 45f),
            (5.1f, 10f),
            (2.5f, 4.5f),

        ];

        Noise2D.calc_2d(texture.as_span(), octaves.as_readonly_span(),texture_size, texture_size);
        texture.update();
        return;

        for (var i = 0; i < texture_size * texture_size; ++i) {
            var px = i % texture_size;
            var py = i / texture_size;

            // sample noise at pixel position

            var height =
                noise.sample(
                             bounds.left + bounds.width * px / texture_size,
                             bounds.bottom + bounds.height * py / texture_size
                            );

            /*var normal = normalize(
                                   float3(
                                          bounds.left + bounds.width * px / texture_size,
                                          height,
                                          bounds.bottom + bounds.height * py / texture_size
                                         )
                                  );*/

            //var hn = packed_height.pack(height, normal);
            //var hn2 = packed_height.unpack(hn);
            //Console.WriteLine($"original={(height, normal)}, unpacked={hn2}");

            // pack height into 2x 8-bit channels
            texture[px, py] = height;
        }

        texture.update();
    }

    public void Dispose() {
        texture.Dispose();
    }
}