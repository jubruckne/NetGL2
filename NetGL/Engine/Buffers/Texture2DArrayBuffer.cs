namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class Texture2DArrayBuffer: TextureBuffer {
    protected readonly Image[] textures;

    public override int capacity => textures.Length;

    public sealed override int item_size { get; }
    public sealed override Type item_type { get; }
    // public override int total_size { get; }

    protected Texture2DArrayBuffer(TextureTarget target, Image[] textures): base(target) {
        handle = 0;
        this.textures = textures;

        length = textures.Length;
        width = textures[0].width;
        height = textures[0].height;
        item_size = textures[0].image_data.Length;
        item_type = textures[0].image_data.GetType();
        //total_size = textures.Length * textures[0].image_data.Length;
    }

    public Texture2DArrayBuffer(in Image[] textures): this(TextureTarget.Texture2DArray, textures) {}

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();
        
        GL.BindTexture(target, handle);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage3D(
            target,
            level: 0,
            PixelInternalFormat.Rgba,
            width,
            height,
            length,
            border: 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            0
        );

        for (int tex_idx = 0; tex_idx < textures.Length; tex_idx++) {
            GL.TexSubImage3D(
                target,
                level: 0,
                xoffset: 0,
                yoffset: 0,
                zoffset: tex_idx,
                width,
                height,
                depth: 1,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                textures[tex_idx].image_data
            );
        }

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

        Debug.assert_opengl();
    }

    public override void update() {
        if (handle == 0)
            Error.not_allocated(this);

        GL.BindTexture(target, handle);
        for (var tex_idx = 0; tex_idx < textures.Length; tex_idx++) {
            GL.TexSubImage3D(
                target,
                level: 0,
                xoffset: 0,
                yoffset: 0,
                zoffset: tex_idx,
                width,
                height,
                depth: 1,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                textures[tex_idx].image_data
            );
        }

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

        Debug.assert_opengl();
    }
}