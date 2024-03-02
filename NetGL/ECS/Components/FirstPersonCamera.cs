using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;

public class FirstPersonCamera: Camera, IComponent<FirstPersonCamera>, IUpdatableComponent {
    private readonly KeyboardState? keyboard_state;
    private readonly MouseState? mouse_state;

    public float speed = 2.5f;
    public float sensitivity = 0.5f;

    internal FirstPersonCamera (
        in Entity entity,
        Viewport viewport,
        float field_of_view = 60f, 
        float aspect_ratio = 4 / 3f, 
        float near = 0.01f, 
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
        ): base(entity, viewport) {
        this.keyboard_state = keyboard_state;
        this.mouse_state = mouse_state;

        projection_matrix = Matrix4.CreatePerspectiveFieldOfView(field_of_view.degree_to_radians(), aspect_ratio, near, far);
        camera_matrix = Matrix4.Identity;
    }

    public override void update(in float game_time, in float delta_time) {
        if (enable_input) {
            var speed = this.speed * delta_time;

            if (keyboard_state != null) {
                if (keyboard_state.IsKeyDown(Keys.W))           // forward
                    transform.move(z: speed);

                if (keyboard_state.IsKeyDown(Keys.S))           // backwards
                    transform.move(z: -speed);

                if (keyboard_state.IsKeyDown(Keys.A))           // left
                    transform.move(x: -speed);

                if (keyboard_state.IsKeyDown(Keys.D))           // right
                    transform.move(x: speed);

                if (keyboard_state.IsKeyDown(Keys.RightShift))  // up
                    transform.move(y: speed);

                if (keyboard_state.IsKeyDown(Keys.LeftShift))   // down
                    transform.move(y: -speed);

                if (keyboard_state.IsKeyDown(Keys.E))
                    transform.roll(1);

                if (keyboard_state.IsKeyDown(Keys.Q))
                    transform.roll(-1);
            }

            if (mouse_state != null) {
                transform.yaw(mouse_state.Delta.X * sensitivity);
                transform.pitch(-mouse_state.Delta.Y * sensitivity);
            }
        }

        /*
        if (transform.attitude.pitch > pitch_clamp[0])
            transform.attitude.pitch = pitch_clamp[0];
        else if (transform.attitude.pitch < pitch_clamp[1])
            transform.attitude.pitch = pitch_clamp[1];
*/
        camera_matrix = transform.calculate_look_at_matrix();

        if (enable_input && enable_update) {
            entity.transform.rotation = transform.rotation;
            entity.transform.position = transform.position;
        }
        /*
        Console.WriteLine("");
        Console.WriteLine("Camera Matrix:");
        Console.WriteLine(camera_matrix.ToString());
        Console.WriteLine("Camera Position:");
        Console.WriteLine(transform.position.ToString());
        Console.WriteLine("Extracted:");
        Console.WriteLine(camera_matrix.ExtractTranslation());
        */
    }

    public override string ToString() {
        return $"position: {entity.transform.position} rotation: {entity.transform.rotation}";
    }
}

public static class FirstPersonCameraExt {
    public static FirstPersonCamera add_first_person_camera(
        this Entity entity,
        Viewport viewport,
        float field_of_view = 60f,
        float aspect_ratio = 4 / 3f,
        float near = 0.01f,
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null,
        bool? enable_input = true,
        bool? enable_update = true,
        float speed = 2.5f,
        float sensitivity = 0.5f
    ) {
        var cam = new FirstPersonCamera(entity, viewport, field_of_view, aspect_ratio, near, far, keyboard_state, mouse_state);
        if (enable_input != null) cam.enable_input = (bool)enable_input;
        if (enable_update != null) cam.enable_update = (bool)enable_update;
        cam.speed = speed;
        cam.sensitivity = sensitivity;
        entity.add(cam);
        return cam;
    }
}