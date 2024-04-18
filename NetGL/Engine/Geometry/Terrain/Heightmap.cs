using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class Heightmap: IDisposable {
    public Rectangle bounds { get; }
    public int texture_size { get; }
    public Texture2D<float> texture { get; }

    public Heightmap(int texture_size, Rectangle bounds) {
        this.bounds       = bounds;
        this.texture_size = texture_size;
        this.texture      = new Texture2D<float>(texture_size, texture_size, PixelFormat.Red, PixelType.Float);
        this.texture.internal_pixel_format = PixelInternalFormat.R32f;
        this.texture.create();
    }

    public void Dispose() {
        texture.Dispose();
    }
}