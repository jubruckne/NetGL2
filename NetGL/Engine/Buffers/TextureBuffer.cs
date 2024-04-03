namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public abstract class TextureBuffer: Buffer, IBindableIndexed {
    protected readonly TextureTarget target;

    public int width { get; protected init; }
    public int height { get; protected init; }
    public int texture_unit { get; private set; } = -1;
    int IBindableIndexed.binding_point => texture_unit;

    public string glsl_type {
        get {
            return target switch {
                TextureTarget.TextureCubeMap => "samplerCube",
                TextureTarget.Texture2D => "sampler2D",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public TextureBuffer(TextureTarget target) {
        this.target = target;
    }

    public void bind(int texture_unit) {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
        GL.BindTexture(target, handle);
    }
}