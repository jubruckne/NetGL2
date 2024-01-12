using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class Texture2DArrayBuffer: Buffer {
    private int handle;
    private readonly Texture[] textures;

    public int width { get; }
    public int height { get; }

    public override int count { get; }
    public override int item_size{ get; }
    public override int size { get; }

    public Texture2DArrayBuffer(in Texture[] textures) {
        handle = 0;

        count = textures.Length;
        width = textures[0].width;
        height = textures[0].height;
        item_size = textures[0].image_data.Length;
        size = textures.Length * textures[0].image_data.Length;
        this.textures = textures;
    }
    
    public override void bind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindTexture(TextureTarget.Texture2DArray, handle);
    }

    public override void unbind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindTexture(TextureTarget.Texture2DArray, 0);
    }

    public override void upload() {
        GL.ActiveTexture(TextureUnit.Texture0);
        if (handle == 0) 
            handle = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2DArray, handle);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage3D(
            TextureTarget.Texture2DArray,
            level: 0,
            PixelInternalFormat.Rgba,
            width,
            height,
            count,
            border: 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            0
        );

        for (int tex_idx = 0; tex_idx < textures.Length; tex_idx++) {
            GL.TexSubImage3D(
                TextureTarget.Texture2DArray,
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

        Error.check();
    }
}