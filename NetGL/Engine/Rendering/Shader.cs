using System.Runtime.InteropServices;

namespace NetGL;

using System.Runtime.CompilerServices;
using ECS;
using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CameraData {
    public static string uniform_name => "Camera";
    public Matrix4 projection_matrix;
    public Matrix4 camera_matrix;
    public Vector3 camera_position;
    private float __padding;
}

public readonly struct UniformBlockDef: INamed {
    public readonly int index;
    public readonly string name;
    public readonly int size_of;

    public UniformBlockDef(int index, string name, int size_of) {
        this.index = index;
        this.name    = name;
        this.size_of = size_of;
    }

    public override string ToString() => $"<UniformBlock index={index}, name={name}, size_of={size_of}/>";

    string INamed.name => this.name;
}

public class Shader: IAssetType<Shader>, IBindable, IEquatable<Shader> {
    static string IAssetType<Shader>.path => "Shaders";

    static Shader IAssetType<Shader>.load_from_file(string path) {
        throw new NotImplementedException();
    }

    int IBindable.handle => this.handle;

    public UniformVariable<Matrix4> model_matrix { get; private set; }
    public UniformBuffer<CameraData> uniform_buffer_camera_data { get; private set; }

    public bool has_geometry_shader { get; private set; }
    public bool has_tesselation_shader { get; private set; }

    public readonly string name;
    public readonly int handle;

    private readonly Dictionary<string, int> uniform_locations;
    private readonly NamedBag<UniformBlockDef> uniform_block_defs;

    public IReadOnlyList<string> uniforms => uniform_locations.Keys.ToList();

    public static readonly IReadOnlyDictionary<int, WeakReference<Shader>> instances = new Dictionary<int, WeakReference<Shader>>();

    protected Shader(string name) {
        this.name = name;
        uniform_locations = [];
        uniform_block_defs = new();

        handle = GL.CreateProgram();
        Shader.instances.writeable().Add(handle, new WeakReference<Shader>(this));
    }

    public static Shader from_file(string name, string vertex_program, string fragment_program) {
        var shader = new Shader(name);
        shader.compile_from_file(vertex_program, fragment_program);
        return shader;
    }

    public static Shader from_file(string name, string vertex_program, string fragment_program, string geometry_program) {
        var shader = new Shader(name);
        shader.compile_from_file(vertex_program, fragment_program, geometry_program);
        return shader;
    }

    public static Shader from_file(string name, string vertex_program, string fragment_program, string tess_control, string tess_eval) {
        var shader = new Shader(name);
        shader.compile_from_file(vertex_program, fragment_program, tess_control_program:tess_control, tess_eval_program:tess_eval);
        return shader;
    }

    public static Shader from_text(string name, string vertex_program, string fragment_program) {
        var shader = new Shader(name);
        shader.compile_from_text(vertex_program, fragment_program);
        return shader;
    }

    public static Shader from_text(string name, string vertex_program, string fragment_program, string geometry_program) {
        var shader = new Shader(name);
        shader.compile_from_text(vertex_program, fragment_program, geometry_program);
        return shader;
    }

    public static Shader from_text(string name, string vertex_program, string fragment_program, string tess_control, string tess_eval) {
        var shader = new Shader(name);
        shader.compile_from_text(vertex_program, fragment_program, tess_control_program:tess_control, tess_eval_program:tess_eval);
        return shader;
    }

    protected void compile_from_text(string vertex_program, string fragment_program, string geometry_program = "", string tess_control_program = "", string tess_eval_program = "") {
        // Console.WriteLine("\ncompiling vertex shader...");
        var vertex_shader_handle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex_shader_handle, vertex_program);
        compile(vertex_shader_handle);

        // Console.WriteLine("compiling fragment shader...");
        var fragment_shader_handle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment_shader_handle, fragment_program);
        compile(fragment_shader_handle);

        var geometry_shader_handle = -1;
        if (geometry_program != "") {
            has_geometry_shader = true;
            // Console.WriteLine("compiling geometry shader...");

            geometry_shader_handle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometry_shader_handle, geometry_program);
            compile(geometry_shader_handle);
        }

        var tess_control_shader_handle = -1;
        if (tess_control_program != "") {
            has_tesselation_shader = true;
            // Console.WriteLine("compiling tesselation control shader...");
            tess_control_shader_handle = GL.CreateShader(ShaderType.TessControlShader);
            GL.ShaderSource(tess_control_shader_handle, tess_control_program);
            compile(tess_control_shader_handle);
        }

        var tess_eval_shader_handle = -1;
        if (tess_eval_program != "") {
            // Console.WriteLine("compiling tesselation control shader...");
            tess_eval_shader_handle = GL.CreateShader(ShaderType.TessEvaluationShader);
            GL.ShaderSource(tess_eval_shader_handle, tess_eval_program);
            compile(tess_eval_shader_handle);
        }

        // Console.WriteLine("attaching shaders...");

        // Attach shaders...
        GL.AttachShader(handle, vertex_shader_handle);

        if(tess_control_shader_handle != -1)
            GL.AttachShader(handle, tess_control_shader_handle);

        if(tess_eval_shader_handle != -1)
            GL.AttachShader(handle, tess_eval_shader_handle);

        if(geometry_shader_handle != -1)
            GL.AttachShader(handle, geometry_shader_handle);

        GL.AttachShader(handle, fragment_shader_handle);

        // Console.WriteLine("linking shaders...");

        LinkProgram(handle);


        GL.DetachShader(handle, vertex_shader_handle);

        if(tess_control_shader_handle != -1)
            GL.DetachShader(handle, tess_control_shader_handle);

        if(tess_eval_shader_handle != -1)
            GL.DetachShader(handle, tess_eval_shader_handle);

        if(geometry_shader_handle != -1)
            GL.DetachShader(handle, geometry_shader_handle);

        GL.DetachShader(handle, fragment_shader_handle);

        GL.DeleteShader(vertex_shader_handle);

        if (tess_control_shader_handle != -1)
            GL.DeleteShader(tess_control_shader_handle);

        if (tess_eval_shader_handle != -1)
            GL.DeleteShader(tess_eval_shader_handle);

        if(geometry_shader_handle != -1)
            GL.DeleteShader(geometry_shader_handle);

        GL.DeleteShader(fragment_shader_handle);

        Debug.assert_opengl();

        // cache uniform locations.
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        uniform_locations.Clear();

        Console.WriteLine("Uniforms: ");

        // Loop over all the uniforms,
        for (var i = 0; i < numberOfUniforms; i++) {
            var key = GL.GetActiveUniform(handle, i, out _, out _);
            var location = GL.GetUniformLocation(handle, key);
            uniform_locations.Add(key, location);

            Console.WriteLine("  " + key + " -> " + location);
        }

        if(has_uniform("model"))
            model_matrix = new(this, "model");

        // cache uniform locations.
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniformBlocks, out var block_count);

        // Loop over all the uniforms,
        for (var i = 0; i < block_count; i++) {
            GL.GetActiveUniformBlockName(handle, i, 128, out var buf_len, out var buffer);
            GL.GetActiveUniformBlock(handle, i, ActiveUniformBlockParameter.UniformBlockDataSize, out var block_size);
            GL.GetActiveUniformBlock(handle, i, ActiveUniformBlockParameter.UniformBlockBinding, out var block_binding);
            string block_name = buffer[..buf_len];
            var block_index = GL.GetUniformBlockIndex(handle, block_name);

            uniform_block_defs.add(new UniformBlockDef(block_index, block_name, block_size));
            Console.WriteLine($"Uniform Block: {i} {uniform_block_defs[0]}");

        }

        if (has_uniform_block(CameraData.uniform_name)) {
            if(uniform_block_defs[CameraData.uniform_name].size_of != Unsafe.SizeOf<CameraData>())
                Error.type_alignment_mismatch<CameraData>(uniform_block_defs[CameraData.uniform_name].size_of, Unsafe.SizeOf<CameraData>());

            uniform_buffer_camera_data = new UniformBuffer<CameraData>(CameraData.uniform_name);
            uniform_buffer_camera_data.create(BufferUsageHint.StaticDraw);
            set_uniform_buffer(uniform_block_defs[CameraData.uniform_name], uniform_buffer_camera_data);
        }
    }

    private void set_uniform_buffer(UniformBlockDef uniform_block_def, IUniformBuffer uniform_buffer) {
        GL.UniformBlockBinding(handle, uniform_block_def.index, uniform_buffer.binding_point);
    }

    private void compile_from_file(string vertex_program, string fragment_program, string geometry_program="", string tess_control_program="", string tess_eval_program="") {
        if (File.Exists(AssetManager.asset_path<Shader>(vertex_program)))
            vertex_program = AssetManager.asset_path<Shader>(vertex_program);
        Console.WriteLine(vertex_program);
        vertex_program = File.ReadAllText(vertex_program);

        if (File.Exists(AssetManager.asset_path<Shader>(fragment_program)))
            fragment_program = AssetManager.asset_path<Shader>(fragment_program);
        Console.WriteLine(fragment_program);
        fragment_program = File.ReadAllText(fragment_program);

        if(geometry_program != "") {
            if (File.Exists(AssetManager.asset_path<Shader>(geometry_program)))
                geometry_program = AssetManager.asset_path<Shader>(geometry_program);
            Console.WriteLine(geometry_program);
            geometry_program = File.ReadAllText(geometry_program);
        }

        if(tess_control_program != "") {
            if (File.Exists(AssetManager.asset_path<Shader>(tess_control_program)))
                tess_control_program = AssetManager.asset_path<Shader>(tess_control_program);
            Console.WriteLine(tess_control_program);
            tess_control_program = File.ReadAllText(tess_control_program);
        }

        if(tess_eval_program != "") {
            if (File.Exists(AssetManager.asset_path<Shader>(tess_eval_program)))
                tess_eval_program = AssetManager.asset_path<Shader>(tess_eval_program);
            Console.WriteLine(tess_eval_program);
            tess_eval_program = File.ReadAllText(tess_eval_program);
        }

        compile_from_text(vertex_program, fragment_program, geometry_program, tess_control_program, tess_eval_program);
    }

    private static void compile(int shader) {
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code == (int)All.True) return;

        var infoLog = GL.GetShaderInfoLog(shader);
        Console.Error.WriteLine();
        Console.Error.WriteLine(infoLog);
        Console.Error.WriteLine();

        throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
    }

    private static void LinkProgram(int program) {
        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
        if (code == (int)All.True) return;

        var result = GL.GetProgramInfoLog(program);
        Console.WriteLine();
        Console.WriteLine(result);
        Console.WriteLine();
        throw new Exception($"Error occurred whilst linking Program({program})");
    }

    public void set_projection_matrix(in Matrix4 matrix) => set_uniform("projection", matrix);
    public void set_camera_matrix(in Matrix4 matrix) => set_uniform("camera", matrix);
    //public void set_model_matrix(in Matrix4 matrix) => set_uniform("model", matrix);
    public void set_game_time(in float game_time) => set_uniform("game_time", game_time);

    public void set_material(Material material) {
        set_uniform("material.ambient_color", material.ambient_color.reinterpret_ref<Color, OpenTK.Mathematics.Vector3>());
        set_uniform("material.diffuse", material.diffuse_color.reinterpret_ref<Color, OpenTK.Mathematics.Vector3>());
        set_uniform("material.specular", material.specular_color.reinterpret_ref<Color, OpenTK.Mathematics.Vector3>());
        set_uniform("material.shininess", material.shininess * 1f);
        if (material.ambient_texture != null) {
            set_uniform("material.ambient_texture", 0);
        }
    }

    public void set_camera_position(in Vector3 pos) {
        set_uniform("cam_position", pos);
    }

    private static class Uniforms {
        public static class DirectionalLight {
            public static readonly string[] Direction = [
                "directional_light[0].direction",
                "directional_light[1].direction",
                "directional_light[2].direction",
                "directional_light[3].direction"
            ];

            public static readonly string[] Ambient = [
                "directional_light[0].ambient",
                "directional_light[1].ambient",
                "directional_light[2].ambient",
                "directional_light[3].ambient"
            ];

            public static readonly string[] Specular = [
                "directional_light[0].specular",
                "directional_light[1].specular",
                "directional_light[2].specular",
                "directional_light[3].specular"
            ];

            public static readonly string[] Diffuse = [
                "directional_light[0].diffuse",
                "directional_light[1].diffuse",
                "directional_light[2].diffuse",
                "directional_light[3].diffuse"
            ];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void set_light(in Light[] lights) {
        var num_directional_lights = 0;

        foreach (var light in lights) {
            switch (light) {
                case AmbientLight ambient:
                    set_uniform("ambient_light", ambient.data.color.reinterpret_ref<Color,Vector3>());
                    break;
                case DirectionalLight directional:
                    set_uniform(Uniforms.DirectionalLight.Direction[num_directional_lights], directional.data.direction);
                    set_uniform(Uniforms.DirectionalLight.Ambient[num_directional_lights], directional.data.ambient.reinterpret_ref<Color, Vector3>());
                    set_uniform(Uniforms.DirectionalLight.Specular[num_directional_lights], directional.data.specular.reinterpret_ref<Color, Vector3>());
                    set_uniform(Uniforms.DirectionalLight.Diffuse[num_directional_lights], directional.data.diffuse.reinterpret_ref<Color, Vector3>());
                    num_directional_lights++;
                    break;
                default:
                    throw new NotImplementedException(light.name);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool has_uniform(string name) => uniform_locations.ContainsKey(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool has_uniform_block(string name) => GL.GetUniformBlockIndex(handle, name) != -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int get_uniform_location(string name) => uniform_locations[name];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, int data) {
        GL.ProgramUniform1(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, float data) {
        GL.ProgramUniform1(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void set_uniform(string name, double data) {
        GL.ProgramUniform1(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, Matrix4 data) {
        GL.ProgramUniformMatrix4(handle, uniform_locations.GetValueOrDefault(name, -1), transpose: true, matrix: ref data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, OpenTK.Mathematics.Vector3 data) {
        GL.ProgramUniform3(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, Color4 data) {
        GL.ProgramUniform4(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, Color data) {
        GL.ProgramUniform4(handle, uniform_locations.GetValueOrDefault(name, -1), data.reinterpret_ref<Color, OpenTK.Mathematics.Vector4>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, Vector2 data) {
        GL.ProgramUniform2(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void set_uniform(string name, Vector4 data) {
        GL.ProgramUniform4(handle, uniform_locations.GetValueOrDefault(name, -1), data);
    }

    public override int GetHashCode() => handle;
    public override bool Equals(object? obj) => Equals(obj as Shader);
    public bool Equals(Shader? other) {
        if (other is null) return false;
        return other.handle == this.handle;
    }

    public override string ToString() => name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void bind() {
        GL.UseProgram(handle);
    }
}