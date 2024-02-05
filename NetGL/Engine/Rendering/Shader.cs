using NetGL.ECS;

namespace NetGL;

using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader {
    public readonly string name;
    private readonly int handle;
    private readonly Dictionary<string, int> uniform_locations;
    public IReadOnlyList<string> uniforms => uniform_locations.Keys.ToList();

    private static Shader? current_shader = null;

    protected Shader(string name) {
        this.name = name;
        uniform_locations = [];
        handle = GL.CreateProgram();
    }

    public Shader(string name, string vertex_program, string fragment_program, string geometry_program = "")
    : this(name) {
        compile_from_file(vertex_program, fragment_program, geometry_program);
    }

    protected void compile_from_text(string vertex_program, string fragment_program, string geometry_program = "") {
        Console.WriteLine("compiling vertex shader...");
        var vertex_shader_handle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex_shader_handle, vertex_program);
        compile(vertex_shader_handle);

        Console.WriteLine("compiling fragment shader...");
        var fragment_shader_handle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment_shader_handle, fragment_program);
        compile(fragment_shader_handle);

        var geometry_shader_handle = -1;
        if (geometry_program != "") {
            Console.WriteLine("compiling geometry shader...");

            geometry_shader_handle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometry_shader_handle, geometry_program);
            compile(geometry_shader_handle);
            GL.AttachShader(handle, geometry_shader_handle);
        }

        // Attach shaders...
        GL.AttachShader(handle, vertex_shader_handle);
        GL.AttachShader(handle, fragment_shader_handle);
        if(geometry_shader_handle != -1)
            GL.AttachShader(handle, geometry_shader_handle);

        // And then link them together.
        Console.WriteLine("linking shaders...");

        LinkProgram(handle);

        GL.DetachShader(handle, vertex_shader_handle);
        GL.DetachShader(handle, fragment_shader_handle);
        if(geometry_shader_handle != -1)
            GL.DetachShader(handle, geometry_shader_handle);

        GL.DeleteShader(fragment_shader_handle);
        GL.DeleteShader(vertex_shader_handle);
        if(geometry_shader_handle != -1)
            GL.DeleteShader(geometry_shader_handle);

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

        Console.WriteLine();
    }

    protected void compile_from_file(string vertex_program, string fragment_program, string geometry_program="") {
        string base_path = $"{AppDomain.CurrentDomain.BaseDirectory}../../../Assets/Shaders/";

        Console.WriteLine("Compiling " + vertex_program + "...\n");

        if(File.Exists(vertex_program))
            vertex_program = File.ReadAllText(vertex_program);
        else if(File.Exists(base_path + vertex_program))
            vertex_program = File.ReadAllText(base_path + vertex_program);
        else
            throw new ArgumentOutOfRangeException(vertex_program, base_path + vertex_program);

        Console.WriteLine("Compiling " + fragment_program + "...\n");
        if(File.Exists(fragment_program))
            fragment_program = File.ReadAllText(fragment_program);
        else if(File.Exists(base_path + fragment_program))
            fragment_program = File.ReadAllText(base_path + fragment_program);
        else
            throw new ArgumentOutOfRangeException(fragment_program);

        if(geometry_program != "") {
            Console.WriteLine("Compiling " + geometry_program + "...\n");

            if(File.Exists(geometry_program))
                geometry_program = File.ReadAllText(geometry_program);
            else if(File.Exists(base_path + geometry_program))
                geometry_program = File.ReadAllText(base_path + geometry_program);
            else
                throw new ArgumentOutOfRangeException(geometry_program);
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

    // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
    // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
    private int GetAttribLocation(string attribName) {
        return GL.GetAttribLocation(handle, attribName);
    }

    public void set_projection_matrix(in Matrix4 matrix) => set_uniform("projection", matrix);
    public void set_camera_matrix(in Matrix4 matrix) => set_uniform("camera", matrix);
    public void set_model_matrix(in Matrix4 matrix) => set_uniform("model", matrix);
    public void set_game_time(in float game_time) => set_uniform("game_time", game_time);

    public void set_material(Material.Color color_material) {
        set_uniform("material.ambient", color_material.ambient.as_vector3());
        set_uniform("material.diffuse", color_material.diffuse.as_vector3());
        set_uniform("material.specular", color_material.specular.as_vector3());
        set_uniform("material.shininess", color_material.shininess);
    }

    public void set_camera_position(in Vector3 pos) {
        set_uniform("cam_position", pos);
    }

    public void set_light(IEnumerable<Light> lights) {
        int num_ambient_lights = 0;
        int num_directional_lights = 0;
        int num_point_lights = 0;

        foreach (var light in lights) {
            switch (light) {
                case AmbientLight ambient:
                    set_uniform("ambient_light", ambient.data.color.as_vector3());
                    num_ambient_lights++;
                    break;
                case DirectionalLight directional:
                    if(directional.data.direction.LengthSquared != 0) directional.data.direction.Normalize();
                    set_uniform($"directional_light[{num_directional_lights}].direction", directional.data.direction);
                    set_uniform($"directional_light[{num_directional_lights}].ambient", directional.data.ambient.as_vector3());
                    set_uniform($"directional_light[{num_directional_lights}].specular", directional.data.specular.as_vector3());
                    set_uniform($"directional_light[{num_directional_lights}].diffuse", directional.data.diffuse.as_vector3());
                    num_directional_lights++;
                    break;
                case PointLight pointlight:
                    break;
                default:
                    throw new NotImplementedException(light.name);
            }
        }
    }

    /// <summary>
    /// Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, int data) {
        GL.UseProgram(handle);
        GL.Uniform1(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, float data) {
        if (has_uniform(name)) {
            GL.UseProgram(handle);
            GL.Uniform1(uniform_locations[name], data);
        }
    }

    private bool has_uniform(in string name) => uniform_locations.ContainsKey(name);

    /// <summary>
    /// Set a uniform double on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, double data) {
        GL.UseProgram(handle);
        GL.Uniform1(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform Matrix4 on this shader
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    /// <remarks>
    ///   <para>
    ///   The matrix is transposed before being sent to the shader.
    ///   </para>
    /// </remarks>
    protected void set_uniform(string name, Matrix4 data) {
        GL.UseProgram(handle);
        GL.UniformMatrix4(uniform_locations[name], transpose:true, matrix:ref data);
    }

    protected bool has_uniform(string name) => uniform_locations.ContainsKey(name);

    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, Vector3 data) {
        GL.UseProgram(handle);
        if (has_uniform(name)) {
            GL.Uniform3(uniform_locations[name], data);
        }
    }

    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, Color4 data) {
        if (has_uniform(name)) {
            GL.UseProgram(handle);
            GL.Uniform4(uniform_locations[name], data);
            Error.check();
        }
    }

    protected void set_uniform(string name, Color4i data) {
        if (has_uniform(name)) {
            GL.UseProgram(handle);
            GL.Uniform4(uniform_locations[name], (Color4)data);

            //Console.WriteLine((Color4)data);
        } else {
            Console.WriteLine($"{this.name} doesn't have uniform {name}!");
        }
    }

    /// <summary>
    /// Set a uniform Vector2 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, Vector2 data) {
        GL.UseProgram(handle);
        GL.Uniform2(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform Vector4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    protected void set_uniform(string name, Vector4 data) {
        GL.UseProgram(handle);
        GL.Uniform4(uniform_locations[name], data);
    }
    
    public override string ToString() {
        return $"{name}";
    }

    public void bind() {
        if (Shader.current_shader != this) {
            Shader.current_shader = this;
            GL.UseProgram(handle);
        }
    }

    public void unbind() {
        Shader.current_shader = null;
        GL.UseProgram(0);
    }

}