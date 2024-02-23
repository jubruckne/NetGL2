using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using NetGL;
using OpenTK.Windowing.Common;
using Vector2 = System.Numerics.Vector2;

public static class ImGuiRenderer {
    private static NativeWindow window = null!;
    private static bool _frameBegun;

    private static int _vertexArray;
    private static int _vertexBuffer;
    private static int _vertexBufferSize;
    private static int _indexBuffer;
    private static int _indexBufferSize;

    private static int _fontTexture;

    private static int _shader = -1;
    private static int _shaderFontTextureLocation = -1;
    private static int _shaderProjectionMatrixLocation = -1;

    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public static void initialize(NativeWindow window, string font = "") {
        ImGuiRenderer.window = window;

        var context = ImGui.CreateContext();

        ImGui.SetCurrentContext(context);
        //ImGuizmo.SetImGuiContext(context);

        var io = ImGui.GetIO();
        Console.WriteLine($"Imgui: {ImGui.GetVersion()}");
        ImFontGlyphRangesBuilderPtr builder;
        unsafe {
            var b = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
            builder = new ImFontGlyphRangesBuilderPtr(b);
        }

        builder.AddChar('\u2192');
        builder.AddRanges(io.Fonts.GetGlyphRangesDefault());
        builder.BuildRanges(out var glyph_ranges);
        var fnt = io.Fonts.AddFontFromFileTTF(font, 17f, null, glyph_ranges.Data);
        fnt.ConfigData.RasterizerDensity = 2f;

        // io.Fonts.AddFontDefault();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        // Enable Docking
        // io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.DpiEnableScaleFonts;
        io.ConfigMacOSXBehaviors = true;

        window.MouseWheel += on_mouse_wheel;
        window.TextInput += on_text_input;

        CreateDeviceResources();
        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();

        _frameBegun = true;
    }

    private static void on_text_input(TextInputEventArgs e) {
        var io = ImGui.GetIO();

        if(io.WantTextInput)
            PressedChars.Add((char)e.Unicode);
    }

    private static void on_mouse_wheel(MouseWheelEventArgs e) {
        var io = ImGui.GetIO();
        io.MouseWheel = e.OffsetY;
        io.MouseWheelH = e.OffsetX;
    }

    private static void CreateDeviceResources() {
        Console.WriteLine($"ImguiController.CreateDeviceResources()");

        _vertexBufferSize = 10240;
        _indexBufferSize = 2048;

        int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);

        _vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        _indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        RecreateFontDeviceTexture();

        string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
gl_Position = projection_matrix * vec4(in_position, 0, 1);
color = in_color;
texCoord = in_texCoord;
}";
        string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
outputColor = color * texture(in_fontTexture, texCoord);
}";

        _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(prevVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

        Error.check();
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    private static void RecreateFontDeviceTexture() {
        Console.WriteLine($"ImguiController.RecreateFontDeviceTexture()");

        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        // Restore state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);

        io.Fonts.SetTexID((IntPtr)_fontTexture);

        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public static void render() {
        if (_frameBegun) {
            _frameBegun = false;

            ImGui.Render();
            RenderImDrawData();
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public static void update(float delta_seconds) {
        if (_frameBegun)
            ImGui.Render();

        SetPerFrameImGuiData(delta_seconds);
        UpdateImGuiInput();

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private static void SetPerFrameImGuiData(float deltaSeconds) {
        var io = ImGui.GetIO();

        var ig_size = io.DisplaySize;
        var fb_size = window.FramebufferSize;

        if (ig_size.X != fb_size.X || ig_size.Y != fb_size.Y) {
            io.DisplaySize = new Vector2(fb_size.X, fb_size.Y);
            io.DisplayFramebufferScale = new Vector2(
                (float)window.FramebufferSize.X / window.ClientSize.X,
                (float)window.FramebufferSize.Y / window.ClientSize.Y
                );

            Console.WriteLine("window resized");

            // Setup orthographic projection matrix into our constant buffer
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                window.ClientSize.X,
                window.ClientSize.Y,
                0.0f,
                -1.0f,
                1.0f);

            if (_shader == 0)
                throw new Error.Exception("_shader == 0");

            GL.ProgramUniformMatrix4(_shader, _shaderProjectionMatrixLocation, false, ref mvp);
            GL.ProgramUniform1(_shader, _shaderFontTextureLocation, 0);
        }

        //Console.WriteLine($"SetPerFrameImGuiData(w:{_windowWidth}, h:{_windowHeight}, scale: {_scaleFactor})");
        Console.WriteLine("DisplaySize = " + io.DisplaySize);
        //Console.WriteLine("DisplayFramebufferScale = " + io.DisplayFramebufferScale);

        io.DeltaTime = deltaSeconds;
    }

    static readonly List<char> PressedChars = new List<char>();

    private static void UpdateImGuiInput() {
        MouseState MouseState = window.MouseState;
        KeyboardState KeyboardState = window.KeyboardState;

        var io = ImGui.GetIO();

        io.MouseDown[0] = MouseState[MouseButton.Left];
        io.MouseDown[1] = MouseState[MouseButton.Right];
        io.MouseDown[2] = MouseState[MouseButton.Middle];
        io.MouseDown[3] = MouseState[MouseButton.Button4];
        io.MouseDown[4] = MouseState[MouseButton.Button5];

        var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
        var point = screenPoint; //wnd.PointToClient(screenPoint);
        io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

        if (io.WantTextInput) {
            foreach (var key in Enum.GetValues<Keys>()) {
                if (key == Keys.Unknown) {
                    continue;
                }

                io.AddKeyEvent(TranslateKey(key), KeyboardState.IsKeyDown(key));
            }

            foreach (var c in PressedChars) {
                io.AddInputCharacter(c);
            }

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        } else PressedChars.Clear();
    }

    private static void RenderImDrawData() {
        ImDrawDataPtr draw_data = ImGui.GetDrawData();

        if (draw_data.CmdListsCount == 0)
            return;

        // Get initial state.
        int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
        int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
        int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
        int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
        bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        Span<int> prevScissorBox = stackalloc int[4];
        unsafe {
            fixed (int* iptr = &prevScissorBox[0]) {
                GL.GetInteger(GetPName.ScissorBox, iptr);
            }
        }

        Span<int> prevPolygonMode = stackalloc int[2];
        unsafe {
            fixed (int* iptr = &prevPolygonMode[0]) {
                GL.GetInteger(GetPName.PolygonMode, iptr);
            }
        }

        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        // Bind the element buffer (thru the VAO) so that we can resize it.
        GL.BindVertexArray(_vertexArray);
        // Bind the vertex buffer so that we can resize it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        for (int i = 0; i < draw_data.CmdListsCount; i++) {
            ImDrawListPtr cmd_list = draw_data.CmdLists[0];

            int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize) {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _vertexBufferSize = newSize;

                Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
            }

            int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize) {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _indexBufferSize = newSize;

                Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
            }
        }

        GL.UseProgram(_shader);

        GL.BindVertexArray(_vertexArray);

        var io = ImGui.GetIO();

        draw_data.ScaleClipRects(io.DisplayFramebufferScale);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Render command lists
        for (int n = 0; n < draw_data.CmdListsCount; n++) {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

            for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++) {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
                    throw new NotImplementedException();

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has
                // flipped Y when it comes to these coordinates
                var clip = pcmd.ClipRect;
                Error.check();
                Console.WriteLine("ImGui: clip = " + clip.ToString());

                GL.Scissor((int)clip.X, (int)(window.FramebufferSize.Y) - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                Error.check();

                if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0) {
                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                } else {
                    GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                }
            }
        }

        // GL.Disable(EnableCap.Blend);
        // GL.Disable(EnableCap.ScissorTest);

        // Reset state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);
        GL.UseProgram(prevProgram);
        GL.BindVertexArray(prevVAO);
        GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
        GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
        GL.BlendFuncSeparate(
            (BlendingFactorSrc)prevBlendFuncSrcRgb,
            (BlendingFactorDest)prevBlendFuncDstRgb,
            (BlendingFactorSrc)prevBlendFuncSrcAlpha,
            (BlendingFactorDest)prevBlendFuncDstAlpha);
        if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
        if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
        if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
        GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);

        Error.check();

    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public static void Dispose() {
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_indexBuffer);

        GL.DeleteTexture(_fontTexture);
        GL.DeleteProgram(_shader);
    }

    private static int CreateProgram(string name, string vertexSource, string fragmentSoruce) {
        int program = GL.CreateProgram();

        int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
        int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0) {
            string info = GL.GetProgramInfoLog(program);
            Console.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
        }

        GL.DetachShader(program, vertex);
        GL.DetachShader(program, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }

    private static int CompileShader(string name, ShaderType type, string source) {
        int shader = GL.CreateShader(type);

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0) {
            string info = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
        }

        return shader;
    }

    private static ImGuiKey TranslateKey(Keys key) {
        if (key >= Keys.D0 && key <= Keys.D9)
            return key - Keys.D0 + ImGuiKey._0;

        if (key >= Keys.A && key <= Keys.Z)
            return key - Keys.A + ImGuiKey.A;

        if (key >= Keys.KeyPad0 && key <= Keys.KeyPad9)
            return key - Keys.KeyPad0 + ImGuiKey.Keypad0;

        if (key >= Keys.F1 && key <= Keys.F12)
            return key - Keys.F1 + ImGuiKey.F12;

        return key switch {
            Keys.Tab => ImGuiKey.Tab,
            Keys.Left => ImGuiKey.LeftArrow,
            Keys.Right => ImGuiKey.RightArrow,
            Keys.Up => ImGuiKey.UpArrow,
            Keys.Down => ImGuiKey.DownArrow,
            Keys.PageUp => ImGuiKey.PageUp,
            Keys.PageDown => ImGuiKey.PageDown,
            Keys.Home => ImGuiKey.Home,
            Keys.End => ImGuiKey.End,
            Keys.Insert => ImGuiKey.Insert,
            Keys.Delete => ImGuiKey.Delete,
            Keys.Backspace => ImGuiKey.Backspace,
            Keys.Space => ImGuiKey.Space,
            Keys.Enter => ImGuiKey.Enter,
            Keys.Escape => ImGuiKey.Escape,
            Keys.Apostrophe => ImGuiKey.Apostrophe,
            Keys.Comma => ImGuiKey.Comma,
            Keys.Minus => ImGuiKey.Minus,
            Keys.Period => ImGuiKey.Period,
            Keys.Slash => ImGuiKey.Slash,
            Keys.Semicolon => ImGuiKey.Semicolon,
            Keys.Equal => ImGuiKey.Equal,
            Keys.LeftBracket => ImGuiKey.LeftBracket,
            Keys.Backslash => ImGuiKey.Backslash,
            Keys.RightBracket => ImGuiKey.RightBracket,
            Keys.GraveAccent => ImGuiKey.GraveAccent,
            Keys.CapsLock => ImGuiKey.CapsLock,
            Keys.ScrollLock => ImGuiKey.ScrollLock,
            Keys.NumLock => ImGuiKey.NumLock,
            Keys.PrintScreen => ImGuiKey.PrintScreen,
            Keys.Pause => ImGuiKey.Pause,
            Keys.KeyPadDecimal => ImGuiKey.KeypadDecimal,
            Keys.KeyPadDivide => ImGuiKey.KeypadDivide,
            Keys.KeyPadMultiply => ImGuiKey.KeypadMultiply,
            Keys.KeyPadSubtract => ImGuiKey.KeypadSubtract,
            Keys.KeyPadAdd => ImGuiKey.KeypadAdd,
            Keys.KeyPadEnter => ImGuiKey.KeypadEnter,
            Keys.KeyPadEqual => ImGuiKey.KeypadEqual,
            Keys.LeftShift => ImGuiKey.LeftShift,
            Keys.LeftControl => ImGuiKey.LeftCtrl,
            Keys.LeftAlt => ImGuiKey.LeftAlt,
            Keys.LeftSuper => ImGuiKey.LeftSuper,
            Keys.RightShift => ImGuiKey.RightShift,
            Keys.RightControl => ImGuiKey.RightCtrl,
            Keys.RightAlt => ImGuiKey.RightAlt,
            Keys.RightSuper => ImGuiKey.RightSuper,
            Keys.Menu => ImGuiKey.Menu,
            _ => ImGuiKey.None
        };
    }
}