using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;

public class OrthographicCamera: Camera, IComponent<FirstPersonCamera>, IUpdatableComponent {
    private readonly KeyboardState? keyboard_state;
    private readonly MouseState? mouse_state;

    public float speed = 2.5f;
    public float sensitivity = 0.5f;

    public readonly Vector2 pitch_clamp = new(20, -20);

    internal OrthographicCamera (
        in Entity entity,
        Viewport viewport,
        int x = 0,
        int y = 0,
        int width = 1,
        int height = 1,
        float near = 0.01f,
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
        ): base(entity, viewport) {
        this.keyboard_state = keyboard_state;
        this.mouse_state = mouse_state;

        projection_matrix = Matrix4.CreateOrthographicOffCenter(x, x+width, y, y+height, near, far);
        camera_matrix = Matrix4.Identity;
    }

    public override string ToString() {
        return $"position: {entity.transform.position} rotation: {entity.transform.rotation}";
    }

    public override void update(in float game_time, in float delta_time) {
        if (enable_input) {
            var speed = this.speed * delta_time;
            var sensitivity = this.sensitivity * this.speed * delta_time * 180f;

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
                transform.rotation.yaw(mouse_state.Delta.X * sensitivity);
                transform.rotation.pitch(-mouse_state.Delta.Y * sensitivity);
            }
        }

        /*
        if (transform.attitude.pitch > pitch_clamp[0])
            transform.attitude.pitch = pitch_clamp[0];
        else if (transform.attitude.pitch < pitch_clamp[1])
            transform.attitude.pitch = pitch_clamp[1];
*/

        entity.transform.copy_from(transform);

        camera_matrix = Matrix4.LookAt(transform.position, transform.position + transform.rotation.forward, transform.rotation.up);
    }
}

public static class OrthographicCameraExt {
    public static OrthographicCamera add_orthographic_camera(
        this Entity entity,
        Viewport viewport,
        int x = 0,
        int y = 0,
        int width = 1,
        int height = 1,
        float near = 0.01f,
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null,
        bool? enable_input = true,
        bool? enable_update = true
    ) {
        var cam = new OrthographicCamera(entity, viewport, x, y, width, height, near, far, keyboard_state, mouse_state);
        if (enable_input != null) cam.enable_input = (bool)enable_input;
        if (enable_update != null) cam.enable_update = (bool)enable_update;
        entity.add(cam);
        return cam;
    }
}