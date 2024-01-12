using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;

public struct FirstPersonCameraComponent: IComponent<FirstPersonCameraComponent> {
    public readonly KeyboardState? keyboard_state;
    public readonly MouseState? mouse_state;
    
    public Matrix4 projection;
    public Vector3 position;

    public readonly float speed = 2.5f;
    public readonly float sensitivity = 0.5f;
    
    public Vector3 up;
    public Vector3 right;
    public Vector3 direction;

    public float yaw = -90;
    public float pitch = 0;
    public readonly Vector2 pitch_clamp = (20, -20);

    public FirstPersonCameraComponent(
        float field_of_view = 60f, 
        float aspect_ratio = 4 / 3f, 
        float near = 0.01f, 
        float far = 1000f,
        Vector3? position = null,
        Vector3? direction = null,
        KeyboardState? keyboard_state = null,
        MouseState? mouse_state = null
        ) {
        this.projection = Matrix4.CreatePerspectiveFieldOfView(field_of_view.degree_to_radians(), aspect_ratio, near, far);
        this.position = position ?? new Vector3(0, 0, 0);        
        this.direction = direction ?? new Vector3(0, 0, 0);
        this.keyboard_state = keyboard_state;
        this.mouse_state = mouse_state;
    }
}