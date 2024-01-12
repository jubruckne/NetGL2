using NetGL;
using NetGL.ECS;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Engine: GameWindow {
    protected readonly World world;

    protected float game_clock;
    protected int frame;

    private double frame_time;
    private int frame_count;

    public Engine(int width, int height, string title): 
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
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (!IsFocused)
            return;

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        var e_time = (float)e.Time;
        
        world.update_systems(e_time);
    }

    protected override void OnLoad() {
        base.OnLoad();

        Entity camera = world.create_entity<TransformComponent, FirstPersonCameraComponent>(
            "Camera",
            new TransformComponent(),
            new FirstPersonCameraComponent(
                position: (0, 0, 250), 
                direction: (0, 0, -1), 
                keyboard_state: KeyboardState, 
                mouse_state: MouseState
            )
        );

        Entity player = world.create_entity<TransformComponent, ParentComponent>(
            "Player", 
            new TransformComponent(), 
            new ParentComponent(camera)
        );

        NetGL.ECS.System cam = world.create_system<FirstPersonCameraSystem>();
        
        GL.Enable(EnableCap.ProgramPointSize);

        Error.check();

        CursorState = CursorState.Grabbed;

        GL.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);

        game_clock = 0;
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        //Console.WriteLine($"frame:{frame}, {GC.CollectionCount(2)}");

        game_clock += (float)e.Time;
        frame_time += e.Time;
        if (frame_time >= 1.0) {
            this.Title = $"fps: {frame_count}, last_frame: {e.Time*1000:F2}";
            frame_count = 0;
            frame_time = 0;
        }

        frame_count++;
        frame++;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        /*
        foreach(Entity ent in entities)  {
            if(ent.is_root()) {
                //Console.WriteLine("Root entity: {0}", ent.name);
                foreach(IEntity child in entities) {
                    //Console.WriteLine("drawing child" + child.name);

                    if(child.parent == ent && child is IRenderable r) {
                        //Console.WriteLine("drawing " + r.ToString());
                        r.shader.bind();
                        
                        Error.check();

                        r.shader.set_uniform("u_projection", camera_controller.projection);
                        r.shader.set_uniform("u_view", child.parent.transform);
                        r.shader.set_uniform("u_model", child.transform);
                        r.mesh.bind();
                        r.mesh.draw();
                    }
                }
            }
        }
        */

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        Console.WriteLine("OnResize: {0} {1}", e.Width, e.Height);
        GL.Viewport(0, 0, e.Width * 2, e.Height * 2);
    }

    protected override void OnUnload() {
        base.OnUnload();
    }
}

