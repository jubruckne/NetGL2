using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;

public class FirstPersonCamera: Camera, IComponent<FirstPersonCamera>, IUpdatableComponent {
    private readonly KeyboardState? keyboard_state;
    private readonly MouseState? mouse_state;

    internal Matrix4 projection_matrix { get; private set; }
    internal Matrix4 camera_matrix { get; private set; }

    public float speed = 2.5f;
    public float sensitivity = 0.5f;

    public float yaw = -90;
    public float pitch = 0;
    public readonly Vector2 pitch_clamp = new(20, -20);

    internal FirstPersonCamera (
        in Entity entity,
        float field_of_view = 60f, 
        float aspect_ratio = 4 / 3f, 
        float near = 0.01f, 
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
        ): base(entity) {
        this.keyboard_state = keyboard_state;
        this.mouse_state = mouse_state;

        projection_matrix = Matrix4.CreatePerspectiveFieldOfView(field_of_view.degree_to_radians(), aspect_ratio, near, far);
        camera_matrix = Matrix4.Identity;
    }

    public override string ToString() {
        return $"yaw: {yaw}, pitch: {pitch}, roll: n/a";
    }

    public override void update(in float game_time, in float delta_time) {
        var speed = this.speed * delta_time;
        var sensitivity = this.sensitivity * this.speed * delta_time * 180f;

        var transform = entity.transform;

        if(keyboard_state != null) {
            if (keyboard_state.IsKeyDown(Keys.W))
                transform.position += transform.forward * speed; //Forward

            if (keyboard_state.IsKeyDown(Keys.S))
                transform.position -= transform.forward * speed; //Backwards

            if (keyboard_state.IsKeyDown(Keys.A))
                transform.position -= Vector3.Normalize(Vector3.Cross(transform.forward, transform.up)) * speed; //Left

            if (keyboard_state.IsKeyDown(Keys.D))
                transform.position += Vector3.Normalize(Vector3.Cross(transform.forward, transform.up)) * speed; //Right

            if (keyboard_state.IsKeyDown(Keys.RightShift))
                transform.position += transform.up * speed; //Up

            if (keyboard_state.IsKeyDown(Keys.LeftShift))
                transform.position -= transform.up * speed; //Down
        }

        if(mouse_state != null) {
            yaw += mouse_state.Delta.X * sensitivity;
            pitch -= mouse_state.Delta.Y * sensitivity;

            if(pitch > pitch_clamp[0])
                pitch = pitch_clamp[0];
            else if(pitch < pitch_clamp[1])
                pitch = pitch_clamp[1];

            transform.forward.X = (float)(Math.Cos(float.DegreesToRadians(pitch))
                                         * Math.Cos(float.DegreesToRadians(yaw)));
            transform.forward.Y = (float)Math.Sin(float.DegreesToRadians(pitch));
            transform.forward.Z = (float)(Math.Cos(float.DegreesToRadians(pitch))
                                         * Math.Sin(float.DegreesToRadians(yaw)));
            transform.forward = Vector3.Normalize(transform.forward);
        }

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        transform.right = Vector3.Normalize(Vector3.Cross(transform.forward, Vector3.UnitY));
        transform.up = Vector3.Normalize(Vector3.Cross(transform.right, transform.forward));

        camera_matrix = Matrix4.LookAt(transform.position, transform.position + transform.forward, transform.up);
    }
}

public static class FirstPersonCameraExt {
    public static FirstPersonCamera add_first_person_camera(
        this Entity entity,
        float field_of_view = 60f,
        float aspect_ratio = 4 / 3f,
        float near = 0.01f,
        float far = 1000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
    ) {
        var cam = new FirstPersonCamera(entity, field_of_view, aspect_ratio, near, far, keyboard_state, mouse_state);
        entity.add(cam);
        return cam;
    }
}