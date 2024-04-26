using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class Heightmap: IDisposable {
    public Rectangle bounds { get; } // in world space
    public int texture_size { get; } // in pixels
    public Texture2D<float> texture { get; }

    public Heightmap(int texture_size, Rectangle bounds) {
        this.bounds       = bounds;
        this.texture_size = texture_size;
        this.texture      = new Texture2D<float>(texture_size, texture_size, PixelFormat.Red, PixelType.Float);
        this.texture.internal_pixel_format = PixelInternalFormat.R32f;
        this.texture.min_filter = TextureMinFilter.Linear;
        this.texture.mag_filter = TextureMagFilter.Linear;
        this.texture.wrap_s = TextureWrapMode.ClampToEdge;
        this.texture.wrap_t = TextureWrapMode.ClampToEdge;
        this.texture.create();
    }

    public void generate(Noise noise) {
        for (var i = 0; i < texture_size * texture_size; ++i) {
            var px = i % texture_size;
            var py = i / texture_size;

            texture[px, py] =
                noise.sample(
                             bounds.left + bounds.width * px / texture_size,
                             bounds.bottom + bounds.height * py / texture_size
                            );
        }

        texture.update();
    }

    public void Dispose() {
        texture.Dispose();
    }
}