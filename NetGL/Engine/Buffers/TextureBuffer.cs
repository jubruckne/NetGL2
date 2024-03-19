using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public abstract class TextureBuffer: Buffer {
    protected readonly TextureTarget target;

    public int width { get; protected init; }
    public int height { get; protected init; }

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

    public override void bind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindTexture(target, handle);
    }
}