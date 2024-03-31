namespace NetGL;

using System.Runtime.CompilerServices;
using ECS;
using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader: IAssetType<Shader>, IBindable, IEquatable<Shader> {
    static string IAssetType<Shader>.path => "Shaders";

    static Shader IAssetType<Shader>.load_from_file(string path) {
        throw new NotImplementedException();
    }

    public readonly string name;
    public int handle { get; init; }
    private readonly Dictionary<string, int> uniform_locations;

    public IReadOnlyList<string> uniforms => uniform_locations.Keys.ToList();

    public static readonly IReadOnlyDictionary<int, WeakReference<Shader>> instances = new Dictionary<int, WeakReference<Shader>>();

    protected Shader(string name) {
        this.name = name;
        uniform_locations = [];
        handle = GL.CreateProgram();
        Shader.instances.writeable().Add(handle, new WeakReference<Shader>(this));
    }

    public Shader(string name, string vertex_program, string fragment_program, string geometry_program = "")
    : this(name) {
        compile_from_file(vertex_program, fragment_program, geometry_program);
    }

    protected void compile_from_text(string vertex_program, string fragment_program, string geometry_program = "") {
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
            // Console.WriteLine("compiling geometry shader...");

            geometry_shader_handle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometry_shader_handle, geometry_program);
            compile(geometry_shader_handle);
        }

        // Console.WriteLine("attaching shaders...");

        // Attach shaders...
        GL.AttachShader(handle, vertex_shader_handle);
        if(geometry_shader_handle != -1)
            GL.AttachShader(handle, geometry_shader_handle);
        GL.AttachShader(handle, fragment_shader_handle);

        // And then link them together.
        // Console.WriteLine("linking shaders...");

        LinkProgram(handle);

        GL.DetachShader(handle, vertex_shader_handle);
        GL.DetachShader(handle, fragment_shader_handle);
        if(geometry_shader_handle != -1)
            GL.DetachShader(handle, geometry_shader_handle);

        GL.DeleteShader(fragment_shader_handle);
        GL.DeleteShader(vertex_shader_handle);
        if(geometry_shader_handle != -1)
            GL.DeleteShader(geometry_shader_handle);

        Debug.assert_opengl();

        // cache uniform locations.
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        uniform_locations.Clear();

        //Console.WriteLine("Uniforms: ");

        // Loop over all the uniforms,
        for (var i = 0; i < numberOfUniforms; i++) {
            var key = GL.GetActiveUniform(handle, i, out _, out _);
            var location = GL.GetUniformLocation(handle, key);
            uniform_locations.Add(key, location);

            //Console.WriteLine("  " + key + " -> " + location);
        }

        //Console.WriteLine();
    }

    private void compile_from_file(string vertex_program, string fragment_program, string geometry_program="") {
        if (File.Exists(AssetManager.asset_path<Shader>(vertex_program)))
            vertex_program = AssetManager.asset_path<Shader>(vertex_program);

        if (File.Exists(AssetManager.asset_path<Shader>(fragment_program)))
            vertex_program = AssetManager.asset_path<Shader>(fragment_program);

        if(geometry_program != "") {
            if (File.Exists(AssetManager.asset_path<Shader>(geometry_program)))
                geometry_program = AssetManager.asset_path<Shader>(geometry_program);
        }

        compile_from_text(vertex_program, fragment_program, geometry_program);

        Console.WriteLine();
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
    public void set_model_matrix(in Matrix4 matrix) => set_uniform("model", matrix);
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

    public void set_light(in IReadOnlyList<Light> lights) {
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
    private bool has_uniform(string name) => uniform_locations.ContainsKey(name);

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