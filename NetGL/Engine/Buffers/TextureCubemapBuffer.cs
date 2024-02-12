using OpenTK.Graphics.OpenGL4;
using ArbTextureFilterAnisotropic = OpenTK.Graphics.OpenGL.ArbTextureFilterAnisotropic;

namespace NetGL;

public class TextureCubemapBuffer: Texture2DArrayBuffer {
    public TextureCubemapBuffer(in Texture right, in Texture left, in Texture top, in Texture bottom, in Texture front,
        in Texture back)
        : base(TextureTarget.TextureCubeMap, [right, left, bottom, top, front, back]) {
    }

    public void upload(TextureUnit texture_unit = TextureUnit.Texture0) {
        GL.ActiveTexture(texture_unit);
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
                internalformat:PixelInternalFormat.Rgba,
                width,
                height,
                border:0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                textures[tex_idx].image_data
            );
        }

        Error.check();
    }
}