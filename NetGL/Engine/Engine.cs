using NetGL;
using NetGL.ECS;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

public class Engine: GameWindow {
    public readonly bool debug;

    protected readonly World world;
    protected readonly ImGuiController imgui_controller = null!;

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
        if (debug) imgui_controller = new ImGuiController(ClientSize.X, ClientSize.Y, 2, 2);

        Attitude att = new();
        att.yaw = 0;
        att.pitch = 10;
        att.roll = 0;
        Console.Write(att.direction);
    }

    protected override void OnLoad() {
        base.OnLoad();

        world.add_ambient_light(0.5f, 0.4f, 0.3f);
        world.add_directional_light((0, 0, -1), Color4.Aqua, Color4.Bisque, Color4.Gold);

        Entity player = world.create_entity("Player");
        player.transform.position = (0, 0, 0);
        player.transform.attitude.direction = (0, 0, -1);
        player.add_first_person_camera(Viewport.Gameplay, field_of_view:75f, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:false);

        Entity hud = world.create_entity("Hud");
        var oc2 = hud.add_orthographic_camera(Viewport.Hud.copy("O2", y:400), x:-2, y:-2, width:4, height:4, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:true);
        var oc4 = hud.add_orthographic_camera(Viewport.Hud.copy("04", x:600, y:400), x:-2, y:-2, width:4, height:4, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:true);

        oc2.transform.position = (0, -1, 0);
        oc2.transform.attitude.direction = (0, 0, 0);

        oc4.transform.position = (0, 0, -1);
        oc4.transform.attitude.direction = (0, 0, 0);


        Entity ball = world.create_sphere_uv("Ball");
        ball.transform.position = (-5, -2, -8);

        Entity box = world.create_cube("Box");
        box.transform.position = (-2, -2, -8);

        Entity rect = world.create_rectangle("Rectangle", divisions:16);
        rect.transform.position = (-.5f, 0.4f, -1.3f);
        rect.add_material(Material.Chrome);
        Console.WriteLine("");

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
                CursorState = CursorState == CursorState.Normal ? CursorState.Grabbed : CursorState.Normal;
                Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
                cursor_state_last_switch = 0f;
                world.for_all_components_with<Camera>(c1 => c1.enable_input = CursorState == CursorState.Grabbed);
            } else if (CursorState == CursorState.Normal) {
                if (KeyboardState.IsKeyDown(Keys.A) |
                    KeyboardState.IsKeyDown(Keys.S) |
                    KeyboardState.IsKeyDown(Keys.D) |
                    KeyboardState.IsKeyDown(Keys.W)) {
                    CursorState = CursorState.Grabbed;
                    Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
                    cursor_state_last_switch = 0f;
                    world.for_all_components_with<Camera>(c1 => c1.enable_input = CursorState == CursorState.Grabbed);
                }
            }
        } else cursor_state_last_switch += delta_time;

        world.update(game_time, delta_time);
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        if (debug) imgui_controller.Update(this, (float)e.Time);

        //Console.WriteLine($"frame:{frame}, {GC.CollectionCount(2)}");

        frame_time += e.Time;
        if (frame_time >= 1.0) {
            Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
            frame_count = 0;
            frame_time = 0;
        }

        frame_count++;
        frame++;

        world.render();
        Viewport.Gameplay.make_current();
        if(debug) render_ui();

        SwapBuffers();
    }

    private void render_ui() {
        // ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());
        ImGui.Begin("Entities",
            CursorState == CursorState.Grabbed
                ? ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
        ImGui.SetWindowSize(new Vector2(260, 680));
        ImGui.SetWindowPos(new Vector2(755, 10));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 12f);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, CursorState == CursorState.Grabbed ? 0.35f : 1f);

        ImGui.StyleColorsClassic();

        add_entities_to_gui(world);

        // ImGui.ShowIDStackToolWindow();

        ImGui.PopStyleVar(4);

        ImGui.End();
        imgui_controller.Render();

        ImGuiController.CheckGLError("End of frame");
    }

    private void add_entities_to_gui(Entity entity) {
        ImGui.PushStyleColor(ImGuiCol.Header, Color4i.random_for(entity.name).to_int());
        if (ImGui.TreeNodeEx(
                entity.name,
                entity.name == "World" | entity.name == "Rectangle"
                    ? ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen
                    : ImGuiTreeNodeFlags.Framed)) {
            ImGui.Unindent();

            foreach (var comp in entity.components) {
                // Console.WriteLine(comp.name);

                if (comp is Hierarchy)
                    continue;
                if (entity is World && comp is Transform)
                    continue;

                if (comp is Transform t) {
                    ImGui.Text("Position");
                    ImGui.SameLine(80);
                    ImGui.DragFloat3($"##{entity.name}.position", ref t.position.as_sys_num_ref(), 0.05f, -100, 100,
                        "%.1f");

                    ImGui.Text("Attitude");
                    ImGui.SameLine(80);
                    ImGui.DragFloat3($"##{entity.name}.attitude",
                        ref t.attitude.yaw_pitch_roll_degrees.as_sys_num_ref(), 1f, -180, 180, "%.0f");

                    ImGui.Spacing();

                    ImGui.Text("Direction");
                    ImGui.SameLine(80);
                    var attitudeDirection = t.attitude.direction;
                    ImGui.InputFloat3($"##{entity.name}.direction", ref attitudeDirection.as_sys_num_ref(), "%.2f",
                        ImGuiInputTextFlags.ReadOnly);

                    ImGui.Text("Up");
                    ImGui.SameLine(80);
                    var attitudeUp = t.attitude.up;
                    ImGui.InputFloat3($"##{entity.name}.up", ref attitudeUp.as_sys_num_ref(), "%.2f",
                        ImGuiInputTextFlags.ReadOnly);

                    ImGui.Spacing();
                    ImGui.Text($"Aizmuth:{t.attitude.azimuth:N1}, Polar: {t.attitude.polar:N1}");
                    ImGui.Spacing();


                } else if (comp is Hierarchy h) {
                    ImGui.Text("Parent");
                    ImGui.SameLine(80);

                    if (ImGui.BeginCombo($"##{entity.name}.parent", h.parent?.name, ImGuiComboFlags.None)) {
                        foreach (var parent_ent in world.children) {
                            ImGui.Selectable(parent_ent.name);
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.Spacing();
                } else if (comp is FirstPersonCamera cam) {
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("Camera");
                    ImGui.Spacing();

                    ImGui.Text("Viewport");
                    ImGui.SameLine(80);
                    ImGui.InputInt4($"##{entity.name}.viewport.pos", ref cam.viewport.x);

                    ImGui.Text("Speed");
                    ImGui.SameLine(80);
                    ImGui.SliderFloat($"##{entity.name}.cam.speed", ref cam.speed, 0, 10);

                    ImGui.Text("Sensitiv.");
                    ImGui.SameLine(80);
                    ImGui.SliderFloat($"##{entity.name}.cam.sensitivity", ref cam.sensitivity, 0, 1);

                    ImGui.Spacing();
                } else if (comp is VertexArrayRenderer va) {
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text(va.name);
                    ImGui.Indent();
                    ImGui.Text($"{va.vertex_array}");

                    ImGui.Text($"{va}");

                    ImGui.Unindent();
                    ImGui.Spacing();
                } else if (comp is ShaderComponent shader) {
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text($"{shader}");

                    ImGui.Spacing();
                } else if (comp is MaterialComponent mat) {
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text("Material --- " + mat.name );
                    ImGui.ColorEdit4($"Ambient##{entity}.{mat.name}.ambient", ref mat.color.ambient.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SameLine(90);
                    ImGui.ColorEdit4($"Diffuse##{entity}.{mat.name}.diffuse", ref mat.color.diffuse.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SameLine(170);
                    ImGui.ColorEdit4($"Specular##{entity}.{mat.name}.specular", ref mat.color.specular.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SliderFloat($"Shininess##{entity}.{mat.name}.shininess", ref mat.color.shininess, 0f, 1f);

                    ImGui.Spacing();
                } else if (comp is AmbientLight amb) {
                    ImGui.Text(comp.name);
                    ImGui.ColorEdit4($"Ambient##{entity}.{comp.name}.color", ref amb.data.color.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.Spacing();
                } else if (comp is DirectionalLight dir) {
                    ImGui.Text(comp.name);

                    ImGui.ColorEdit4($"Ambient##{entity}.{comp.name}.ambient", ref dir.data.ambient.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SameLine(90);
                    ImGui.ColorEdit4($"Diffuse##{entity}.{comp.name}.diffuse", ref dir.data.diffuse.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SameLine(170);
                    ImGui.ColorEdit4($"Specular##{entity}.{comp.name}.specular", ref dir.data.specular.as_sys_num_ref(), ImGuiColorEditFlags.NoInputs);
                    ImGui.SliderFloat3($"Direction##{entity}.{comp.name}.direction", ref dir.data.direction.as_sys_num_ref(), -1, 1);
                    ImGui.Spacing();
                } else {
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text(comp.name);
                    ImGui.SameLine(80);
                    ImGui.Spacing();
                }
            }

            foreach (var child in entity.children) {
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

        if (debug) imgui_controller.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnTextInput(TextInputEventArgs e) {
        base.OnTextInput(e);

        if(debug) imgui_controller.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) {
        base.OnMouseWheel(e);
        if(debug) imgui_controller.MouseScroll(e.Offset);
    }

    protected override void OnUnload() {
        base.OnUnload();
    }
}