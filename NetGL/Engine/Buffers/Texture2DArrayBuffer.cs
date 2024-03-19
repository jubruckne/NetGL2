using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class Texture2DArrayBuffer: TextureBuffer {
    protected readonly Texture[] textures;

    public override int length { get; }
    public override int item_size { get; }
    public override Type item_type { get; }
    public override int total_size { get; }

    protected Texture2DArrayBuffer(TextureTarget target, in Texture[] textures): base(target) {
        handle = 0;

        length = textures.Length;
        width = textures[0].width;
        height = textures[0].height;
        item_size = textures[0].image_data.Length;
        item_type = textures[0].image_data.GetType();
        total_size = textures.Length * textures[0].image_data.Length;
        this.textures = textures;
    }

    public Texture2DArrayBuffer(in Texture[] textures): this(TextureTarget.Texture2DArray, textures) {}

    public override void upload() {
        upload(TextureUnit.Texture0);
    }

    public virtual void upload(TextureUnit texture_unit) {
        GL.ActiveTexture(texture_unit);
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

        Error.assert_opengl();
    }
}