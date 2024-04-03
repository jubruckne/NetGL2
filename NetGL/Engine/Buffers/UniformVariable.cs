namespace NetGL;

using ECS;
using OpenTK.Graphics.OpenGL4;

public interface IUniform;

public class UniformVariable<T>: IUniform, INamed where T: unmanaged {
    private T uniform_value;

    public T data {
        get => uniform_value;
        set {
            if (uniform_value.is_equal_to(in value)) return;
            uniform_value = value;
            update_and_make_current();
        }
    }

    public string name { get; }
    private readonly int location;
    private readonly Shader shader;

    public UniformVariable(in Shader shader, string name, in T value) {
        this.shader   = shader;
        this.name     = name;
        this.location = shader.get_uniform_location(name);
        this.data     = value;
    }

    public UniformVariable(in Shader shader, string name): this(shader, name, default) {}

    private void update_and_make_current() {
        var handle = shader.handle;

        switch (uniform_value) {
            case int i:
                GL.ProgramUniform1(handle, location, i);
                break;
            case float f:
                GL.ProgramUniform1(handle, location, f);
                break;
            case OpenTK.Mathematics.Vector3 v3:
                GL.ProgramUniform3(handle, location, ref v3);
                break;
            case OpenTK.Mathematics.Matrix4 m4:
                GL.ProgramUniformMatrix4(handle, location, true, ref m4);
                break;
        }
    }
}