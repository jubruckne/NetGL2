using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class ShaderBuilder {
    private readonly string name;
    private readonly Dictionary<ShaderType, int> stages = [];

    public static ShaderBuilder create(in string name) {
        return new ShaderBuilder(name);
    }

    private ShaderBuilder(in string name) {
        this.name = name;
    }

    public ShaderBuilder add_vertex_shader(in string source) {
        if(stages.ContainsKey(ShaderType.VertexShader))
            Error.duplicated_key(ShaderType.VertexShader);

        stages[ShaderType.VertexShader] = compile(ShaderType.VertexShader, source);

        return this;
    }

    public ShaderBuilder add_fragment_shader(in string source) {
        if(stages.ContainsKey(ShaderType.FragmentShader))
            Error.duplicated_key(ShaderType.FragmentShader);

        stages[ShaderType.FragmentShader] = compile(ShaderType.FragmentShader, source);

        return this;
    }

    public ShaderBuilder add_geometry_shader(in string source) {
        if(stages.ContainsKey(ShaderType.GeometryShader))
            Error.duplicated_key(ShaderType.GeometryShader);

        stages[ShaderType.GeometryShader] = compile(ShaderType.GeometryShader, source);

        return this;
    }

    public ShaderBuilder add_tesselation_control_shader(in string source) {
        if(stages.ContainsKey(ShaderType.TessControlShader))
            Error.duplicated_key(ShaderType.TessControlShader);

        stages[ShaderType.TessControlShader] = compile(ShaderType.TessControlShader, source);

        return this;
    }

    public ShaderBuilder add_tesselation_evaluation_shader(in string source) {
        if(stages.ContainsKey(ShaderType.TessEvaluationShader))
            Error.duplicated_key(ShaderType.TessEvaluationShader);

        stages[ShaderType.TessEvaluationShader] = compile(ShaderType.TessEvaluationShader, source);

        return this;
    }

    private static int compile(ShaderType stage, string source) {
        var handle = GL.CreateShader(stage);
        GL.ShaderSource(handle, source);
        GL.CompileShader(handle);
        var info = GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(info)) {
            throw new Exception($"Error compiling shader: {info}");
        }

        return handle;
    }

    public int link() {
        var handle = GL.CreateProgram();

        foreach (var stage in stages) {
            GL.AttachShader(handle, stage.Value);
        }

        GL.LinkProgram(handle);
        var info = GL.GetProgramInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(info)) {
            throw new Exception($"Error linking shader program: {info}");
        }

        foreach (var stage in stages) {
            GL.DetachShader(handle, stage.Value);
            GL.DeleteShader(stage.Value);
        }

        return handle;
    }
}