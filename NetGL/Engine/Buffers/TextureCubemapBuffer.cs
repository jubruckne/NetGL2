namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class TextureCubemapBuffer: Texture2DArrayBuffer {
    public TextureCubemapBuffer(in Image right, in Image left, in Image top, in Image bottom, in Image front,
        in Image back)
        : base(TextureTarget.TextureCubeMap, [right, left, bottom, top, front, back]) {
    }

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.Enable(EnableCap.TextureCubeMapSeamless);

        for (int tex_idx = 0; tex_idx < textures.Length; tex_idx++) {
            GL.TexImage2D(
                TextureTarget.TextureCubeMapPositiveX + tex_idx,
                level: 0,
                internalformat:PixelInternalFormat.CompressedRgb,
                width,
                height,
                border:0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                textures[tex_idx].image_data
            );
        }

        Debug.assert_opengl();
    }

    public override void update() {
        if(handle == 0)
            Error.not_allocated(this);

        GL.BindTexture(target, handle);

        for (int tex_idx = 0; tex_idx < textures.Length; tex_idx++) {
            GL.TexSubImage2D(
                TextureTarget.TextureCubeMapPositiveX + tex_idx,
                level: 0,
                xoffset: 0,
                yoffset: 0,
                width,
                height,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                textures[tex_idx].image_data
            );
        }

        Debug.assert_opengl();
    }
}