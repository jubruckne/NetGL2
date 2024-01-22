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

    public Shader(string name, string vertex_program, string fragment_program, string geometry_program="") {
        this.name = name;
        Console.WriteLine("Compiling " + vertex_program + "...\n");

        string base_path = $"{AppDomain.CurrentDomain.BaseDirectory}../../../Assets/Shaders/";

        if(File.Exists(vertex_program))
            vertex_program = File.ReadAllText(vertex_program);
        else if(File.Exists(base_path + vertex_program))
            vertex_program = File.ReadAllText(base_path + vertex_program);
        else
            throw new ArgumentOutOfRangeException(vertex_program, base_path + vertex_program);

        // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);

        // Now, bind the GLSL source code
        GL.ShaderSource(vertexShader, vertex_program);

        // And then compile
        compile(vertexShader);

        Console.WriteLine("Compiling " + fragment_program + "...\n");
        if(File.Exists(fragment_program))
            fragment_program = File.ReadAllText(fragment_program);
        else if(File.Exists(base_path + fragment_program))
            fragment_program = File.ReadAllText(base_path + fragment_program);
        else
            throw new ArgumentOutOfRangeException(fragment_program);

        // We do the same for the fragment shader.
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragment_program);
        compile(fragmentShader);

        // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
        // To do this, create a program...

        handle = GL.CreateProgram();

        int handle_geo = -1;

        if(geometry_program != "") {
            Console.WriteLine("Compiling " + geometry_program + "...\n");

            if(File.Exists(geometry_program))
                geometry_program = File.ReadAllText(geometry_program);
            else if(File.Exists(base_path + geometry_program))
                geometry_program = File.ReadAllText(base_path + geometry_program);
            else
                throw new ArgumentOutOfRangeException(geometry_program);

            // We do the same for the fragment shader.
            handle_geo = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(handle_geo, geometry_program);
            compile(handle_geo);

            GL.AttachShader(handle, handle_geo);
        }

        // Attach shaders...
        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        if(handle_geo != -1)
            GL.AttachShader(handle, handle_geo);

        // And then link them together.
        LinkProgram(handle);

        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        if(handle_geo != -1)
            GL.DetachShader(handle, handle_geo);

        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
        if(handle_geo != -1)
            GL.DeleteShader(handle_geo);

        // cache uniform locations.
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        uniform_locations = [];

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

    private static void compile(int shader) {
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code == (int)All.True) return;
        
        var infoLog = GL.GetShaderInfoLog(shader);
        Console.WriteLine();
        Console.WriteLine(infoLog);
        Console.WriteLine();
        
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

    // Uniform setters
    // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
    // You use VBOs for vertex-related data, and uniforms for almost everything else.

    // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
    //     1. Bind the program you want to set the uniform on
    //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
    //     3. Use the appropriate GL.Uniform* function to set the uniform.

    /// <summary>
    /// Set a uniform int on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, int data) {
        GL.UseProgram(handle);
        GL.Uniform1(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform float on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, float data) {
        GL.UseProgram(handle);
        GL.Uniform1(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform double on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, double data) {
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
    public void set_uniform(string name, Matrix4 data) {
        GL.UseProgram(handle);
        GL.UniformMatrix4(uniform_locations[name], transpose:true, matrix:ref data);
    }

    public bool has_uniform(string name) => uniform_locations.ContainsKey(name);

    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, Vector3 data) {
        GL.UseProgram(handle);
        GL.Uniform3(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform Vector3 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, Color4 data) {
        GL.UseProgram(handle);
        GL.Uniform4(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform Vector2 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, Vector2 data) {
        GL.UseProgram(handle);
        GL.Uniform2(uniform_locations[name], data);
    }

    /// <summary>
    /// Set a uniform Vector4 on this shader.
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <param name="data">The data to set</param>
    public void set_uniform(string name, Vector4 data) {
        GL.UseProgram(handle);
        GL.Uniform4(uniform_locations[name], data);
    }
    
    public override string ToString() {
        return $"{name}";
    }

    public void bind() {
        GL.UseProgram(handle);
    }

    public void unbind() {
        GL.UseProgram(0);
    }
}