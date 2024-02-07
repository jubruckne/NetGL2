using ImGuiNET;
using NetGL;
using NetGL.ECS;
using NetGL.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

public class Engine: GameWindow {
    public readonly bool debug;

    protected readonly World world;
    protected readonly ImGuiController handler_imgui = null!;

    private RevolvingList<float> frame_times = new(100);
    protected float game_time;
    protected int frame;

    private float cursor_state_last_switch = 1f;
    private double frame_time;
    private int frame_count;

    public Engine(int width, int height, string title, bool debug = false):
        base(
            new() {
                UpdateFrequency = 0,
            }, new() {
                APIVersion = new Version("4.1"),
                AlphaBits = 8,
                NumberOfSamples = 16,
                ClientSize = (width, height),
                Title = title,
                WindowState = WindowState.Normal,
                Vsync = VSyncMode.On,
                Flags = ContextFlags.ForwardCompatible
            }) {

        this.debug = debug;

        Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Shading Language Version: {GL.GetString(StringName.ShadingLanguageVersion)}");
        Console.WriteLine($"Extensions: {GL.GetString(StringNameIndexed.Extensions, 0)}");
        Console.WriteLine();
        Console.WriteLine($"MaxElementsVertices: {GL.GetInteger(GetPName.MaxElementsVertices)}");
        Console.WriteLine($"MaxElementsIndices: {GL.GetInteger(GetPName.MaxElementsIndices)}");
        Console.WriteLine();

        GL.CullFace(CullFaceMode.Back);
        GL.Enable(EnableCap.CullFace);

        //GL.Enable(EnableCap.Blend);
        //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        world = new World();
        if (debug) handler_imgui = new ImGuiController(ClientSize.X, ClientSize.Y, 2, 2, "/System/Library/Fonts/Supplemental/Arial Unicode.ttf");
    }

    protected override void OnLoad() {
        base.OnLoad();
/*
        var desc = VertexDescriptor.make(NetGL.dev.VertexAttribute.Position, NetGL.dev.VertexAttribute.Normal);
        desc.allocate(100);
        Console.WriteLine(desc);
        desc.buffer.

        return;*/
        world.add_ambient_light(0.5f, 0.5f, 0.5f);
        world.add_directional_light(
            direction:(0, 0, -1),
            ambient:new(55, 55, 55, 255),
            diffuse:new(0.75f, 0.6f, 0.60f, 1f),
            specular:new(0.25f, 0.25f, 0.2f, 1f)
            );

        Entity player = world.create_entity("Player");
        player.transform.position = (0, 0, 10);
        player.transform.attitude.direction = (0, 0, -1);
        player.add_first_person_camera(Viewport.Gameplay, field_of_view:70f, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:false);

        /* Entity hud = world.create_entity("Hud");
        var oc2 = hud.add_orthographic_camera(Viewport.Hud.copy("O2", x:25, y:25), x:-2, y:-2, width:4, height:4, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:true);
        var oc4 = hud.add_orthographic_camera(Viewport.Hud.copy("04", x:25, y:350), x:-2, y:-2, width:4, height:4, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:true);

        oc2.transform.position = (0, -1, 0);
        oc2.transform.attitude.direction = (0, 0, 0);

        oc4.transform.position = (0, 0, -1);
        oc4.transform.attitude.direction = (0, 0, 0); */

        Entity ball = world.create_sphere_uv("Ball");
        ball.transform.position = (-5, -2, -8);

        Entity rect = world.create_rectangle("Rectangle", divisions:16);
        rect.transform.position = (0, 0, 0);
        rect.add_material(Material.Chrome);
        Console.WriteLine("");

       // Entity entd = world.create_model("74656", Model.from_file("1701d.fbx")); //"74656.glb")); // "1701d.fbx"));
        Entity entd = world.create_model("74656", Model.from_file("DragonAttenuation.glb"));

        entd.transform.position = (-4, 0, 0);
        entd.transform.attitude.yaw = -120;
        entd.transform.attitude.pitch = -5f;
        entd.transform.attitude.roll = 2.5f;


        GL.Enable(EnableCap.ProgramPointSize);

        Error.check();

        CursorState = CursorState.Normal;

        game_time = 0f;
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (!IsFocused)
            return;

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        var delta_time = (float)e.Time;
        game_time += delta_time;

        if (KeyboardState.IsKeyDown(Keys.Space)) {
            world.for_all_components_with<Camera>(camera => camera.viewport.resize(1000, 800, 800, 600));
        }

        if (cursor_state_last_switch >= 1f) {
            if (KeyboardState.IsKeyDown(Keys.Tab)) {
                grabbed = !grabbed;
                cursor_state_last_switch = 0f;
            } else if (CursorState == CursorState.Normal) {
                if (KeyboardState.IsKeyDown(Keys.A) |
                    KeyboardState.IsKeyDown(Keys.S) |
                    KeyboardState.IsKeyDown(Keys.D) |
                    KeyboardState.IsKeyDown(Keys.W)) {

                    grabbed = true;
                }
            }
        } else cursor_state_last_switch += delta_time;

        world.update(game_time, delta_time);
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        //Console.WriteLine($"frame:{frame}, {GC.CollectionCount(2)}");

        frame_time += e.Time;

        if (frame_time >= 1f) {
            frame_times.add((float)(frame_time/frame_count) * 10f);

            Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
            frame_count = 0;
            frame_time = 0;
        }

        frame_count++;
        frame++;

        if (debug) handler_imgui.Update(this, (float)e.Time);

        world.render();
        Viewport.Gameplay.make_current();
        if(debug) render_ui();

        SwapBuffers();
    }

    private void render_ui() {
        // ImGui.ShowMetricsWindow();
        // ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());
        ImGui.Begin("Entities",
            CursorState == CursorState.Grabbed
                ? ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
        ImGui.SetWindowSize(new Vector2(260, 680));
        ImGui.SetWindowPos(new Vector2(755, 10));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 12f);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, CursorState == CursorState.Grabbed ? 0.35f : 1f);

        ImGui.StyleColorsClassic();

        if (frame_times.count != 0) {
            ImGui.Text("Frame Time (ms):");
            ImGui.PlotLines($"##frame: {frame_times.last * 100f:F2}  ", ref frame_times.as_span()[0], frame_times.count);
            ImGui.PlotHistogram($"##fradme: {frame_times.last * 100f:F2}  ", ref frame_times.as_span()[0], frame_times.count);
        }

        add_entities_to_gui(world);

        // ImGui.ShowIDStackToolWindow();

        ImGui.PopStyleVar(4);

        ImGui.End();
        handler_imgui.Render();

        ImGuiController.CheckGLError("End of frame");
    }

    private void add_entities_to_gui(Entity entity) {
        ImGui.PushStyleColor(ImGuiCol.Header, Color4i.random_for(entity.name).to_int());
        if (ImGui.TreeNodeEx(
                $"{entity.name}##{entity.get_path()}",
                entity.name == "World" | entity.name == "74656"
                    ? ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen
                    : ImGuiTreeNodeFlags.Framed, entity.name)) {
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Color4(65, 55, 65, 255).to_int());
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Color4(65, 55, 65, 255).to_int());
            ImGui.PushStyleColor(ImGuiCol.Header, new Color4(30, 25, 28, 255).to_int());
            ImGui.Unindent();

            foreach (var comp in entity.components) {
                // Console.WriteLine(comp.name);

                if (entity is World && comp is Transform)
                    continue;

                if (comp is Transform t) {
                    if (ImGui.TreeNodeEx($"{t.name}##{entity.name}_{t.name}_node", entity.name == "74656" ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None)) {
                        ImGui.Unindent();

                        //ImGui2.Joystick(25);
                        ImGui.Joystick2(t, 25);

                        ImGui.DragFloat3($"Position##{entity.name}.position", ref t.position.as_sys_num_ref(), 0.05f, -100, 100, "%.1f");

                        ImGui.DragFloat3($"Attitude##{entity.name}.attitude",
                            ref t.attitude.yaw_pitch_roll_degrees.as_sys_num_ref(), 1f, -180, 180, "%.0f");

                        ImGui.Spacing();

                        var attitudeDirection = t.attitude.direction;
                        ImGui.DragFloat3($"Direction→##{entity.name}.direction", ref attitudeDirection.as_sys_num_ref(), 0f, -1, 1, "%.1f",
                            ImGuiSliderFlags.NoInput);

                        var attitudeUp = t.attitude.up;
                        ImGui.DragFloat3($"Up→##{entity.name}.up", ref attitudeUp.as_sys_num_ref(), 0f, -1, 1, "%.1f",
                            ImGuiSliderFlags.NoInput);

                        //ImGui.Spacing();
                        //ImGui.Text($"Aizmuth:{t.attitude.azimuth:N1}, Polar: {t.attitude.polar:N1}");
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is FirstPersonCamera cam) {
                    if (ImGui.TreeNodeEx($"{cam.name}##{entity.name}_{cam.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();

                        var camEnableInput = cam.enable_input;
                        ImGui.Checkbox("Enable Input", ref camEnableInput);
                        var camEnableUpdate = cam.enable_update;
                        ImGui.Checkbox("Enable Update", ref camEnableUpdate);

                        ImGui.InputInt4($"Viewport##{entity.name}.viewport.pos", ref cam.viewport.x);
                        ImGui.SliderFloat($"Speed##{entity.name}.cam.speed", ref cam.speed, 0, 10);
                        ImGui.SliderFloat($"Sensitivity##{entity.name}.cam.sensitivity", ref cam.sensitivity, 0, 1);

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is VertexArrayRenderer var) {
                    if (ImGui.TreeNodeEx($"{var.name}##{entity.name}_{var.name}_node", ImGuiTreeNodeFlags.None)) {
                        ImGui.Unindent();

                        foreach (var va in var.vertex_arrays) {
                            ImGui.Text($"{va.primitive_type} ({va.vertex_buffers.Length})");
                            va.vertex_buffers.for_each(buffer => ImGui.TextWrapped(buffer.ToString()));
                            ImGui.Text(va.ToString());
                            ImGui.Spacing();
                        }

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is ShaderComponent shader) {
                    if (ImGui.TreeNodeEx($"{shader.name}##{entity.name}_{shader.name}_node", ImGuiTreeNodeFlags.None)) {
                        ImGui.Unindent();
                        ImGui.Text($"{shader}");
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is MaterialComponent mat) {
                    if (ImGui.TreeNodeEx($"{mat.name}##{entity.name}_{mat.name}_node", ImGuiTreeNodeFlags.None, $"{mat.name.Replace("Component", "")}: {mat.material.name}")) {
                        ImGui.Unindent();

                        ImGui.ColorEdit3($"Ambient##{entity}.{mat.name}.ambient",
                            ref mat.material.ambient.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.ColorEdit3($"Diffuse##{entity}.{mat.name}.diffuse",
                            ref mat.material.diffuse.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.ColorEdit3($"Specular##{entity}.{mat.name}.specular",
                            ref mat.material.specular.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.SliderFloat($"Shininess##{entity}.{mat.name}.shininess", ref mat.material.shininess, -1, 1);

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is AmbientLight amb) {
                    if (ImGui.TreeNodeEx($"{amb.name}##{entity.name}_{amb.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();
                        ImGui.ColorEdit3($"Ambient##{entity}.{comp.name}.ambi.color",
                            ref amb.data.color.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is DirectionalLight dir) {
                    if (ImGui.TreeNodeEx($"{dir.name}##{entity.name}_{dir.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();
                        ImGui.ColorEdit3($"Ambient##{entity}.{comp.name}.ambient", ref dir.data.ambient.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.ColorEdit3($"Diffuse##{entity}.{comp.name}.diffuse", ref dir.data.diffuse.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.ColorEdit3($"Specular##{entity}.{comp.name}.specular", ref dir.data.specular.as_sys_num_ref3(), ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.SliderFloat3($"Direction→##{entity}.{comp.name}.direction", ref dir.data.direction.as_sys_num_ref(), -1, 1);
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else {
                    ImGui.Spacing();
                    ImGui.Text("***" + comp.name + "***");
                    ImGui.Spacing();
                }

                ImGui.Spacing();
            }

            ImGui.PopStyleColor(3);

            foreach (var child in entity.children) {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                add_entities_to_gui(child);
            }

            ImGui.Indent();
            ImGui.TreePop();
        }

        ImGui.PopStyleColor();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        //Console.WriteLine("OnResize: {0} {1}", e.Width, e.Height);
        Viewport.Gameplay.resize(0, 0, e.Width * 2, e.Height * 2);

        if (debug) handler_imgui.WindowResized(ClientSize.X, ClientSize.Y);
    }

    private bool grabbed {
        get => CursorState == CursorState.Grabbed;
        set {
            if (value) {
                CursorState = CursorState.Grabbed;
                UpdateFrequency = 0;
                world.for_all_components_with<Camera>(c1 => c1.enable_input = true);
            } else {
                CursorState = CursorState.Normal;
                UpdateFrequency = 30;
                world.for_all_components_with<Camera>(c1 => c1.enable_input = false);
            }
        }
    }

    protected override void OnTextInput(TextInputEventArgs e) {
        base.OnTextInput(e);
        if(debug && !grabbed) handler_imgui.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) {
        base.OnMouseWheel(e);
        if(debug) handler_imgui.MouseScroll(e.Offset);
    }

    protected override void OnUnload() {
        base.OnUnload();
    }
}