namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class Texture2DBuffer: TextureBuffer {
    private readonly Image _image;

    public override int capacity => 1;

    public override int item_size { get; }
    public override Type item_type { get; }
    // public override int total_size { get; }

    public Texture2DBuffer(Image image) : base(TextureTarget.Texture2D) {
        handle = 0;

        width = image.width;
        height = image.height;
        length = 1;
        item_size = image.image_data.Length;
        item_type = image.image_data.GetType();
        this._image = image;
    }

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage2D(
            target,
            level: 0,
            PixelInternalFormat.CompressedRgb,
            width,
            height,
            border: 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            _image.image_data
        );

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Debug.assert_opengl();
    }

    public override void update() {
        if (handle == 0)
            Error.not_allocated(this);

        GL.BindTexture(target, handle);

        GL.TexSubImage2D(
            target,
            level: 0,
            xoffset: 0,
            yoffset: 0,
            width,
            height,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            _image.image_data
        );

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Debug.assert_opengl();
    }
}