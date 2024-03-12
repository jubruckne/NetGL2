using System.Numerics;
using BulletSharp;
using ImGuiNET;
using NetGL;
using NetGL.Debug;
using NetGL.ECS;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Plane = NetGL.Plane;
using Vector2 = System.Numerics.Vector2;

public class Engine: GameWindow {
    public readonly bool debug;

    public readonly World world;

    private readonly RevolvingList<float> frame_times = new(60);
    protected float game_time;
    protected int frame;

    private float cursor_state_last_switch = 1f;
    private double frame_time;
    private int frame_count;

    public Engine(string title, Size2<int> window_size, WindowState window_state = WindowState.Normal, bool debug = false) :
        base(
            new() {
                UpdateFrequency = 0,
            }, new() {
                APIVersion = new Version("4.1"),
                AlphaBits = 8,
                NumberOfSamples = 16,
                ClientSize = window_size,
                Title = title,
                WindowState = window_state,
                Vsync = VSyncMode.On,
                Flags = ContextFlags.ForwardCompatible
            }) {

        this.debug = debug;

        Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Shading Language Version: {GL.GetString(StringName.ShadingLanguageVersion)}");
        Console.WriteLine($"Vendor: {GL.GetString(StringName.Vendor)}");
        Console.WriteLine($"Renderer: {GL.GetString(StringName.Renderer)}");
        Console.WriteLine($"ContextFlags: {(ContextFlagMask)GL.GetInteger(GetPName.ContextFlags)}");
        Console.WriteLine();

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
        if (debug) {
            ImGuiRenderer.initialize(this, "/System/Library/Fonts/Supplemental/Arial Unicode.ttf");
            DebugConsole.initialize();
           // DebugConsole.text_filter = "ImGui";
        }
    }

    protected override void OnLoad() {
        base.OnLoad();

        Viewport.Gameplay.resize(0, 0, FramebufferSize.X, FramebufferSize.Y);


/*

        phy.Update(0.1f);
        Console.WriteLine(phy.World.NumCollisionObjects);

*/
        /*
        TextureCubemapBuffer cubemap = new(
            Texture.load_from_file("test.png"),
            Texture.load_from_file("test.png"),
            Texture.load_from_file("test.png"),
            Texture.load_from_file("test.png"),
            Texture.load_from_file("test.png"),
            Texture.load_from_file("test.png")
        );*/

        TextureCubemapBuffer cubemap = new(
            AssetManager.load_from_file<Texture>("right.jpg"),
            AssetManager.load_from_file<Texture>("left.jpg"),
            AssetManager.load_from_file<Texture>("top.jpg"),
            AssetManager.load_from_file<Texture>("bottom.jpg"),
            AssetManager.load_from_file<Texture>("front.jpg"),
            AssetManager.load_from_file<Texture>("back.jpg")
        );
        cubemap.upload();

/*
        var desc = VertexDescriptor.make(NetGL.dev.VertexAttribute.Position, NetGL.dev.VertexAttribute.Normal);
        desc.allocate(100);
        Console.WriteLine(desc);
        desc.buffer.

        return;*/

        var mat = Material.Chrome;
        mat.ambient_texture = cubemap;
        Entity environment = world.create_cube("Environment", material:mat, radius:100f);
        //((environment.get<VertexArrayRenderer>().vertex_arrays[0] as VertexArrayIndexed).index_buffer).reverse_winding();
       //((environment.get<VertexArrayRenderer>().vertex_arrays[0] as VertexArrayIndexed).index_buffer).upload();
       environment.get<VertexArrayRenderer>().front_facing = false;
       environment.get<VertexArrayRenderer>().depth_test = false;
       environment.get<VertexArrayRenderer>().cull_face = true;

        world.add_directional_light(
            ambient:(0.4f, 0.4f, 0.4f),
            diffuse:(0.9f, 0.9f, 0.9f),
            specular:(1.0f, 1.0f, 1.0f)
            ).data.direction.set_azimuth_altitude(90, 45);

        Entity player = world.create_entity("Player");
        player.transform.position = (0, +200, 0);
        player.transform.rotation = Rotation.Down;

        player.add_first_person_camera(Viewport.Gameplay, field_of_view:70f, keyboard_state: KeyboardState, mouse_state: MouseState, enable_input:false, speed:28f, sensitivity:0.75f);
/*
        var oc1 = player.add_first_person_camera(Viewport.Hud.copy("O2", x:325, y:200), field_of_view:60f, enable_input:false);
        var oc2 = player.add_first_person_camera(Viewport.Hud.copy("O2", x:125, y:25), field_of_view:60f, enable_input:false);
        var oc3 = player.add_first_person_camera(Viewport.Hud.copy("O2", x:25, y:200), field_of_view:60f, enable_input:false);
        var oc4 = player.add_first_person_camera(Viewport.Hud.copy("04", x:125, y:350), field_of_view:60f, enable_input:false);

        oc1.transform.position = (1, 2, 20);
        oc1.transform.attitude.direction = (0, -1, 0);

        oc2.transform.position = (1, 2, 20);
        oc2.transform.attitude.direction = (1, 0, 0);

        oc3.transform.position = (1, 2, 20);
        oc3.transform.attitude.direction = (0, 1, 0);

        oc4.transform.position = (1, 2, 20);
        oc4.transform.attitude.direction = (-1, 0, 0);
*/

        var planet_mat = new Material("planet", Color.Black, Color.White, Color.White, 2.5f);
        Texture2DBuffer planets_texture = new Texture2DBuffer(AssetManager.load_from_file<Texture>("8k_jupiter.jpg"));
        planets_texture.upload();

        planet_mat.ambient_texture = planets_texture;

/*
        Entity ball = world.create_sphere_cube("Ball2", radius: 10f, segments:100, material: Material.Emerald);
        ball.transform.position = (-5, 15, -15);
//        ((ball.get<VertexArrayRenderer>().vertex_arrays[0] as VertexArrayIndexed).index_buffer).reverse_winding();
//        ((ball.get<VertexArrayRenderer>().vertex_arrays[0] as VertexArrayIndexed).index_buffer).upload();
        ball.get<VertexArrayRenderer>().wireframe = false;
        ball.get<VertexArrayRenderer>().cull_face = true;
        ball.get<VertexArrayRenderer>().depth_test = true;
        ball.get<VertexArrayRenderer>().blending = false;
        //ball.add_rigid_body(radius:5f, mass:10000f);
*/
        //ball.transform.position = (0, 3, 5);
        //Texture2DArrayBuffer tex = new Texture2DArrayBuffer([Texture.load_from_file("test.png")]);

        Console.WriteLine("");

        Entity real_arrow_x = world.create_arrow("ArrowX", from: (0, 0, 0), to: (10, 0, 0), material:Material.Red);
        Entity real_arrow_y = world.create_arrow("ArrowY", from: (0, 0, 0), to: (0f, 10, 0f), material:Material.Green);
        Entity real_arrow_z = world.create_arrow("ArrowZ", from: (0, 0, 0), to: (0f, 0f, 10f), material:Material.Blue);

       // Entity entd = world.create_model("74656", Model.from_file("1701d.fbx")); //"74656.glb")); // "1701d.fbx"));
       /*var arrow = Model.from_file("ArrowPointer.obj", 0.75f);

       Entity arrow_x = world.create_model("ArrowX", arrow, material:Material.Red);
       arrow_x.transform.position = new OpenTK.Mathematics.Vector3(1, 0, 0);
       arrow_x.transform.rotation = Rotation.Right;

       Entity arrow_y = world.create_model("ArrowY", arrow, material:Material.Green);
       arrow_y.transform.position = new OpenTK.Mathematics.Vector3(2, 0, 0);
       arrow_y.transform.rotation = Rotation.Up;

       Entity arrow_z = world.create_model("ArrowZ", arrow, material:Material.Blue);
       arrow_z.transform.position = new OpenTK.Mathematics.Vector3(3, 0, 0);
       arrow_z.transform.rotation = Rotation.Forward;

       Console.WriteLine($"x: {arrow_x.transform}, fw:{arrow_x.transform.rotation.forward}, rt:{arrow_x.transform.rotation.right}, up:{arrow_x.transform.rotation.up}");
       Console.WriteLine($"y: {arrow_y.transform}, fw:{arrow_y.transform.rotation.forward}, rt:{arrow_y.transform.rotation.right}, up:{arrow_y.transform.rotation.up}");
       Console.WriteLine($"z: {arrow_z.transform}, fw:{arrow_z.transform.rotation.forward}, rt:{arrow_z.transform.rotation.right}, up:{arrow_z.transform.rotation.up}");

*/

       Entity terrain1 = world.create_terrain(Plane.XZ);


/*
       Entity entd = world.create_model("dragon", Model.from_file("DragonAttenuation.glb", 1f)); // ""));

        entd.transform.position = (-4, -4, 0);
        entd.transform.rotation.yaw_pitch_roll = (-120, -5f, 2.5f);

        var con = new Predicate<Entity>(entity => entity.transform.position.Y < -2.75f);
        var beh = new Action<Entity>(entity => {
            entity.transform.position.randomize(-2.5f, 2.5f).add(x: -1.5f, y: 16, 5.5f);
            entity.get<Component<RigidBody>>().data.LinearVelocity = Vector3.Zero;
        });

        foreach (var b in Enumerable.Range(1, 35)) {
            Entity cube = world.create_sphere_uv($"Sphere{b}", radius:0.20f, material:Material.random);
            cube.transform.position.randomize(-2.5f, 2.5f).add(x:-1.5f, y:15, 5.5f + Random.Shared.NextSingle() * 20f);

            cube.add_rigid_body(radius:0.20f, mass:1f);
            cube.add_behavior(con, beh);
        }
*/
        GL.Enable(EnableCap.ProgramPointSize);

        AssetManager.load_all_files<Script>();
        AssetManager.for_each<Script>(script => script.run(this));

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

        BackgroundTaskScheduler.process_completed_tasks();

        world.update(game_time, delta_time);
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        //Console.WriteLine($"frame:{frame}, {GC.CollectionCount(2)}");

        frame_time += e.Time;

        if (frame_time >= 1f) {
            if(game_time > 2f)
                frame_times.add((float)(frame_time/frame_count));

            Title = $"fps: {frame_count}, last_frame: {e.Time * 1000:F2} - {CursorState}";
            frame_count = 0;
            frame_time = 0;
        }

        frame_count++;
        frame++;

        if (debug) ImGuiRenderer.update((float)e.Time);

        Viewport.Gameplay.make_current();

        world.render();
        Error.check();
        if(debug) render_ui();
        Error.check();

        BackgroundTaskScheduler.process_scheduled_tasks();

        SwapBuffers();
    }

    private void render_ui() {
        // ImGui.ShowMetricsWindow();
        // ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());

        Vector2 consolePos = new Vector2(ClientSize.X * 0.82f, 0);
        Vector2 consoleSize = new Vector2(ClientSize.X * 0.18f, ClientSize.Y);

        ImGui.SetNextWindowPos(consolePos);
        ImGui.SetNextWindowSize(consoleSize);

        ImGui.Begin("Entities",
            CursorState == CursorState.Grabbed
                ? ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 12f);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, CursorState == CursorState.Grabbed ? 0.35f : 1f);

        ImGui.StyleColorsClassic();

        Error.check();

        if (frame_times.count > 5) {
            ImGui.Text("Frame Time (ms):");
            ImGui.PlotHistogram(
                $"##frame_time",
                ref frame_times.as_span()[0],
                frame_times.count,
                0,
                $"avg:{frame_times.average() * 1000:F1}, min:{frame_times.minimum() * 1000:F1}, max:{frame_times.maximum() * 1000:F1}",
                scale_min:frame_times.minimum(),
                scale_max:frame_times.maximum(),
                new Vector2(240, 80));
        }

        DebugConsole.draw();

        add_entities_to_gui(world);

        // ImGui.ShowIDStackToolWindow();

        ImGui.PopStyleVar(4);
        ImGui.End();
        Error.check();

        ImGuiRenderer.render();
    }

    private void add_entities_to_gui(Entity entity) {
        ImGui.PushStyleColor(ImGuiCol.Header, Color.random_for(entity.name).to_int());
        if (ImGui.TreeNodeEx(
                $"{entity.name}##{entity.path}",
                entity.name == "World" | entity.name == "Player"
                    ? ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen
                    : ImGuiTreeNodeFlags.Framed, entity.name)) {
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, Color.make(55, 65, 255).to_int());
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, Color.make(65, 55, 65, 255).to_int());
            ImGui.PushStyleColor(ImGuiCol.Header, Color.make(30, 25, 28, 255).to_int());
            ImGui.Unindent();

            foreach (var comp in entity.components) {
                // Console.WriteLine(comp.name);

                if (entity is World && comp is Transform)
                    continue;

                if (comp is Transform t) {
                    if (ImGui.TreeNodeEx($"{t.name}##{entity.name}_{t.name}_node", entity.name == "74656" ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None)) {
                        ImGui.Unindent();

                        //ImGui2.Joystick(25);
                        // ImGui.Joystick2(t, 25);

                        ImGui.DragFloat3($"Position##{entity.name}.position", ref t.position.reinterpret_ref<OpenTK.Mathematics.Vector3, System.Numerics.Vector3>(), 0.05f, -100, 100, "%.1f");

                        var rotationYawPitchRoll = t.rotation.yaw_pitch_roll;
                            new System.Numerics.Vector3(t.rotation.yaw, t.rotation.yaw, t.rotation.roll);
                        if(ImGui.DragFloat3($"Rotation##{entity.name}.rot",
                                ref rotationYawPitchRoll.reinterpret_ref<OpenTK.Mathematics.Vector3, Vector3>(), 1f, -180,
                                180, "%.0f")) {
                            t.rotation.yaw_pitch_roll = rotationYawPitchRoll;
                        }

                        ImGui.Spacing();

                        var attitudeDirection = t.rotation.forward;
                        ImGui.DragFloat3($"Direction→##{entity.name}.direction", ref attitudeDirection.reinterpret_ref<OpenTK.Mathematics.Vector3, System.Numerics.Vector3>(), 0f, -1, 1, "%.1f",
                            ImGuiSliderFlags.NoInput);

                        var attitudeUp = t.rotation.up;
                        ImGui.DragFloat3($"Up→##{entity.name}.up", ref attitudeUp.reinterpret_ref<OpenTK.Mathematics.Vector3, System.Numerics.Vector3>(), 0f, -1, 1, "%.1f",
                            ImGuiSliderFlags.NoInput);

                        ImGui.Spacing();

                        //ImGui.Text($"Aizmuth:{t.attitude.azimuth:N1}, Polar: {t.attitude.polar:N1}");
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is FirstPersonCamera cam) {
                    if (ImGui.TreeNodeEx($"{cam.name}##{entity.name}_{cam.name}_node",
                            ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();

                        var camEnableInput = cam.enable_input;
                        if (ImGui.Checkbox("Enable Input", ref camEnableInput)) {
                            cam.enable_input = camEnableInput;
                        }

                        var camEnableUpdate = cam.enable_update;
                        if (ImGui.Checkbox("Enable Update", ref camEnableUpdate)) {
                            cam.enable_update = camEnableUpdate;
                        }

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
                            va.vertex_buffers.for_each(buffer => ImGui.TextWrapped(buffer.ToString() ?? string.Empty));
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
                            ref mat.material.ambient_color.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.ColorEdit3($"Diffuse##{entity}.{mat.name}.diffuse",
                            ref mat.material.diffuse_color.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.ColorEdit3($"Specular##{entity}.{mat.name}.specular",
                            ref mat.material.specular_color.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions);
                        ImGui.SliderFloat($"Shininess##{entity}.{mat.name}.shininess", ref mat.material.shininess, 0, 100);

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is AmbientLight amb) {
                    if (ImGui.TreeNodeEx($"{amb.name}##{entity.name}_{amb.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();
                        ImGui.ColorEdit3($"Ambient##{entity}.{comp.name}.ambi.color",
                            ref amb.data.color.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is DirectionalLight dir) {
                    if (ImGui.TreeNodeEx($"{dir.name}##{entity.name}_{dir.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();
                        ImGui.ColorEdit3($"Ambient##{entity}.{comp.name}.ambient", ref dir.data.ambient.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.ColorEdit3($"Diffuse##{entity}.{comp.name}.diffuse", ref dir.data.diffuse.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        ImGui.ColorEdit3($"Specular##{entity}.{comp.name}.specular", ref dir.data.specular.vector3, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoInputs);
                        //ImGui.SliderFloat3($"Direction→##{entity}.{comp.name}.direction", ref dir.data.direction.reinterpret_cast<OpenTK.Mathematics.Vector3, System.Numerics.Vector3>(), -1, 1);

                        var (azimuth, altitude) = dir.data.direction.to_azimuth_altitude();

                        if (ImGui.DragFloat($"Azimuth##{entity.name}.azimuth", ref azimuth, 1f, -181f, 181f, "%.1f")
                           || ImGui.DragFloat($"Altitude##{entity.name}.altitude", ref altitude, 1f, -90f, 90f, "%.1f")) {
                            dir.data.direction.set_azimuth_altitude(azimuth, altitude);
                        }

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is Component<Physics> phy) {
                    if (ImGui.TreeNodeEx($"{phy.name}##{entity.name}_node", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();

                        ImGui.Text($"World type: {phy.data.World.WorldType}");
                        ImGui.Text($"Physics objects: {phy.data.World.NumCollisionObjects}");

                        var worldGravity = phy.data.World.Gravity;
                        if (ImGui.DragFloat3($"Gravity", ref worldGravity, 0.1f, -10, 10, "%.2f")) {
                            Console.WriteLine(worldGravity);
                            phy.data.World.SetGravity(ref worldGravity);
                            phy.data.World.ApplyGravity();
                            phy.data.World.ClearForces();
                            phy.data.World.SynchronizeMotionStates();
                        }

                        ImGui.Indent();
                        ImGui.TreePop();
                    }
                } else if (comp is Component<RigidBody> body) {
                    if (ImGui.TreeNodeEx($"RigidBody {body.name}##{entity.name}.{body.name}", ImGuiTreeNodeFlags.DefaultOpen)) {
                        ImGui.Unindent();

                        ImGui.Text($"CenterOfMassPosition: {body.data.CenterOfMassPosition}");

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
        Viewport.Gameplay.resize(0, 0, e.Width * 2, e.Height * 2);
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

    protected override void OnUnload() {
        base.OnUnload();
    }
}