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

    public override string ToString() {
        return $"position: {entity.transform.position} rotation: {entity.transform.rotation}";
    }

    public override void update(in float game_time, in float delta_time) {
        if (enable_input) {
            var speed = this.speed * delta_time;

            if (keyboard_state != null) {
                if (keyboard_state.IsKeyDown(Keys.W))
                    transform.position += transform.rotation.forward * speed; //Forward

                if (keyboard_state.IsKeyDown(Keys.S))
                    transform.position -= transform.rotation.forward * speed; //Backwards

                if (keyboard_state.IsKeyDown(Keys.A))
                    transform.position -= Vector3.Normalize(transform.rotation.right) * speed; //Left

                if (keyboard_state.IsKeyDown(Keys.D))
                    transform.position += Vector3.Normalize(transform.rotation.right) * speed; //Right

                if (keyboard_state.IsKeyDown(Keys.RightShift))
                    transform.position += transform.rotation.up * speed; //Up

                if (keyboard_state.IsKeyDown(Keys.LeftShift))
                    transform.position -= transform.rotation.up * speed; //Down
            }

            if (mouse_state != null) {
                Console.WriteLine($"mdx: {mouse_state.Delta.X:F6}, sens:{sensitivity:F6}");

                Console.WriteLine($"{transform.rotation} + {(mouse_state.Delta.X * sensitivity):F8}");
                Console.WriteLine($"ypr before: {transform.rotation.yaw_pitch_roll}");

                transform.rotation.yaw(mouse_state.Delta.X * sensitivity);
                Console.WriteLine($"ypr after : {transform.rotation.yaw_pitch_roll}");

                transform.rotation.pitch(-mouse_state.Delta.Y * sensitivity);
            }
        }

        /*
        if (transform.attitude.pitch > pitch_clamp[0])
            transform.attitude.pitch = pitch_clamp[0];
        else if (transform.attitude.pitch < pitch_clamp[1])
            transform.attitude.pitch = pitch_clamp[1];
*/
        camera_matrix = Matrix4.LookAt(transform.position, transform.position + transform.rotation.forward, transform.rotation.up);

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
}

public static class FirstPersonCameraExt {
    public static FirstPersonCamera add_first_person_camera(
        this Entity entity,
        Viewport viewport,
        float field_of_view = 60f,
        float? aspect_ratio = null,
        float near = 0.01f,
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null,
        bool? enable_input = true,
        bool? enable_update = true,
        float speed = 2.5f,
        float sensitivity = 0.5f
    ) {
        var cam = new FirstPersonCamera(entity, viewport, field_of_view, aspect_ratio ?? viewport.aspect_ratio, near, far, keyboard_state, mouse_state);
        if (enable_input != null) cam.enable_input = (bool)enable_input;
        if (enable_update != null) cam.enable_update = (bool)enable_update;
        cam.speed = speed;
        cam.sensitivity = sensitivity;
        entity.add(cam);
        return cam;
    }
}