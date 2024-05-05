using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;

public class FirstPersonCamera: Camera, IComponent<FirstPersonCamera>, IUpdatableComponent {
    private readonly KeyboardState? keyboard_state;
    private readonly MouseState? mouse_state;

    public float speed = 2.5f;
    public float sensitivity = 0.5f;
    public readonly float field_of_view_degrees;
    public readonly float near;
    public readonly float far;
    public Frustum frustum { get; private set; }

    internal FirstPersonCamera (
        in Entity entity,
        Viewport viewport,
        float field_of_view,
        float aspect_ratio,
        float near = 0.01f, 
        float far = 10000f,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
        ): base(entity, viewport) {
        this.keyboard_state = keyboard_state;
        this.mouse_state = mouse_state;

        this.field_of_view_degrees = field_of_view;
        this.near = near;
        this.far = far;

        camera_data.projection_matrix = Matrix4.CreatePerspectiveFieldOfView(field_of_view.degree_to_radians(), aspect_ratio, near, far);
        camera_data.camera_matrix = Matrix4.Identity;
        frustum = Frustum.from_matrix(camera_data.get_view_projection_matrix());
    }

    public override void update(float delta_time) {
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

        camera_data.projection_matrix = Matrix4.CreatePerspectiveFieldOfView(field_of_view_degrees.degree_to_radians(), viewport.aspect_ratio, near, far);
        camera_data.camera_matrix = transform.calculate_look_at_matrix();
        frustum = Frustum.from_matrix(camera_data.get_view_projection_matrix());
        //camera_data.game_time = Engine.game_time;
        camera_data.camera_position = transform.position;

        if (enable_input && enable_update) {
            entity.transform.rotation = transform.rotation;
            entity.transform.position = transform.position;
        }
    }

    public float calculate_resolution_at_distance(float distance) {
        var fov = field_of_view_degrees.degree_to_radians();

        var screen_width_at_distance = 2f * MathF.Tan(fov / 2f) * distance;
        return viewport.width / screen_width_at_distance;
    }

    public float calculate_size_at_distance(float size, float distance) {
        // Convert the field of view from degrees to radians
        var fov = field_of_view_degrees.degree_to_radians();

        // Calculate the width of the view at the distance
        var screen_width_at_distance = 2f * MathF.Tan(fov / 2f) * distance;

        // Determine what fraction of the screen width the object covers
        return size / screen_width_at_distance * viewport.width;
    }

    public override string ToString() => $"position: {entity.transform.position} rotation: {entity.transform.rotation}";
}

public static class FirstPersonCameraExt {
    public static FirstPersonCamera add_first_person_camera(
        this Entity entity,
        Viewport viewport,
        float field_of_view = 60f,
        float? aspect_ratio = null,
        float near = 0.01f,
        float far = 10000f,
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