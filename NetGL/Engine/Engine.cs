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

        world.add_ambient_light(new Color4(0.5f, 0.1f, 0.1f, 1.0f));
        world.add_directional_light((0, 0, -1), Color4.Aqua, Color4.Bisque, Color4.Gold);

        Entity player = world.create_entity("Player");
        player.transform.position = (0, 0, 0);
        player.transform.forward = (0, 0, -1);
        player.add_first_person_camera(field_of_view:75f, keyboard_state: KeyboardState, mouse_state: MouseState);

        Entity ball = world.create_sphere_uv("Ball");
        ball.transform.position = (-5, -2, -8);

        Entity box = world.create_cube("Box");
        box.transform.position = (-2, -2, -8);

        Entity rect = world.create_rectangle("Rectangle", divisions:16);
        rect.transform.position = (-.5f, 0.4f, -1.3f);
        rect.add_material(Material.Gold);

        Console.WriteLine("");
        Console.WriteLine($"player: parents: {player.parents.to_print()}");
        Console.WriteLine($"ball:   parents: {ball.parents.to_print()}");
        Console.WriteLine($"world self: {world.get_entities(Entity.EntityRelationship.Self).to_print()}");
        Console.WriteLine($"world child: {world.get_entities(Entity.EntityRelationship.Children).to_print()}");
        Console.WriteLine($"player par: {player.get_entities(Entity.EntityRelationship.Parent).to_print()}");
        Console.WriteLine($"box    par: {box.get_entities(Entity.EntityRelationship.Parent).to_print()}");
        Console.WriteLine($"box    pars: {box.get_entities(Entity.EntityRelationship.ParentsRecursive).to_print()}");
        Console.WriteLine($"world childs: {world.get_entities(Entity.EntityRelationship.ChildrenRecursive).to_print()}");

        Console.WriteLine("");

        //NetGL.ECS.System cam = world.create_system<FirstPersonCameraSystem>();
        //NetGL.ECS.System rend = world.create_system<VertexArrayRenderSystem>();

        GL.Enable(EnableCap.ProgramPointSize);

        Error.check();

        CursorState = CursorState.Normal;

        GL.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);

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
                world.for_all_components_with<Camera>(c1 => c1.enable_updates = CursorState == CursorState.Grabbed);
            } else if (CursorState == CursorState.Normal) {
                if (KeyboardState.IsKeyDown(Keys.A) |
                    KeyboardState.IsKeyDown(Keys.S) |
                    KeyboardState.IsKeyDown(Keys.D) |
                    KeyboardState.IsKeyDown(Keys.W)) {
                    CursorState = CursorState.Grabbed;
                    Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
                    cursor_state_last_switch = 0f;
                    world.for_all_components_with<Camera>(c1 => c1.enable_updates = CursorState == CursorState.Grabbed);
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

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        world.render();
        if(debug) render_ui();

        SwapBuffers();
    }

    private void render_ui() {
        // ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());
        ImGui.Begin("Entities",
            CursorState == CursorState.Grabbed
                ? ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
        ImGui.SetWindowSize(new Vector2(250, 680));
        ImGui.SetWindowPos(new Vector2(765, 10));

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
        // Console.WriteLine(entity.name);
        ImGui.CollapsingHeader(entity.name, ImGuiTreeNodeFlags.Leaf);

        foreach (var comp in entity.get_components()) {
            // Console.WriteLine(comp.name);

            if(entity is World && comp is Transform or Hierarchy)
                continue;

            if (comp is Transform t) {
                ImGui.Text("Attitude");
                ImGui.SameLine(80);
                ImGui.DragFloat3($"##{entity.name}.attitude", ref t.attitute.yaw_pitch_roll_degrees.as_sys_num_ref(), 1f, -180, 180);
                ImGui.Spacing();

                ImGui.Text("Position");
                ImGui.SameLine(80);
                ImGui.DragFloat3($"##{entity.name}.position", ref t.position.as_sys_num_ref(), 0.05f, -100, 100);

                ImGui.Text("Forward");
                ImGui.SameLine(80);
                ImGui.DragFloat3($"##{entity.name}.forward", ref t.forward.as_sys_num_ref(), 0.01f, -1, 1);

                ImGui.Text("Up");
                ImGui.SameLine(80);
                ImGui.DragFloat3($"##{entity.name}.up", ref t.up.as_sys_num_ref(), 0.01f, -1, 1);
/*
                ImGui.Spacing();

                Matrix4 mat;
                if (entity.has<FirstPersonCamera>())
                    mat = entity.get<FirstPersonCamera>().camera_matrix;
                else
                    mat = t.get_model_matrix();

                var x1 = t.get_model_matrix().Row0;
                ImGui.SliderFloat4($"##{entity.name}.mat1", ref x1.as_sys_num_ref(), 0, 1);
                var x2 = t.get_model_matrix().Row1;
                ImGui.SliderFloat4($"##{entity.name}.mat2", ref x2.as_sys_num_ref(), 0, 1);
                var x3 = t.get_model_matrix().Row2;
                ImGui.SliderFloat4($"##{entity.name}.mat3", ref x3.as_sys_num_ref(), 0, 1);
                var x4 = t.get_model_matrix().Row3;
                ImGui.SliderFloat4($"##{entity.name}.mat4", ref x4.as_sys_num_ref(), 0, 1);
              */  ImGui.Spacing();
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

                ImGui.Text("Material");
                ImGui.Indent();
                ImGui.Text($"{mat.color}");

                ImGui.Unindent();
                ImGui.Spacing();
            } else if (comp is AmbientLight amb) {
                ImGui.Text(comp.name);
                ImGui.ColorEdit4($"##{entity}.ambient.color", ref amb.data.color.as_sys_num_ref());
                ImGui.Spacing();
            } else if (comp is DirectionalLight dir) {
                ImGui.Text(comp.name);
                ImGui.ColorEdit4($"##{entity}.directional.ambient", ref dir.data.ambient.as_sys_num_ref());
                ImGui.ColorEdit4($"##{entity}.directional.diffuse", ref dir.data.diffuse.as_sys_num_ref());
                ImGui.ColorEdit4($"##{entity}.directional.specular", ref dir.data.specular.as_sys_num_ref());
                ImGui.SliderFloat3($"##{entity}.directional.direction", ref dir.data.direction.as_sys_num_ref(), -1, 1);
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
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        //Console.WriteLine("OnResize: {0} {1}", e.Width, e.Height);
        world.for_all_components_with<Camera>(camera => camera.viewport.resize(0, 0, e.Width * 2, e.Height * 2));

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