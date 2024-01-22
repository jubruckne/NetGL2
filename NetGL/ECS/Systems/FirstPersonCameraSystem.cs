using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NetGL.ECS;
/*
public class FirstPersonCameraSystem: System<TransformComponent, FirstPersonCamera> {
    public FirstPersonCameraSystem(): base("First Person Camera", update) { }

    private static void update(ref TransformComponent transform, ref FirstPersonCamera camera, in float delta_time) {
        var speed = camera.speed * delta_time;
        var sensitivity = camera.sensitivity * camera.speed * delta_time * 180f;

        if(camera.keyboard_state != null) {
            if (camera.keyboard_state.IsKeyDown(Keys.W))
                camera.position += camera.direction * speed; //Forward 

            if (camera.keyboard_state.IsKeyDown(Keys.S))
                camera.position -= camera.direction * speed; //Backwards

            if (camera.keyboard_state.IsKeyDown(Keys.A))
                camera.position -= Vector3.Normalize(Vector3.Cross(camera.direction, camera.up)) * speed; //Left

            if (camera.keyboard_state.IsKeyDown(Keys.D))
                camera.position += Vector3.Normalize(Vector3.Cross(camera.direction, camera.up)) * speed; //Right

            if (camera.keyboard_state.IsKeyDown(Keys.RightShift))
                camera.position += camera.up * speed; //Up 

            if (camera.keyboard_state.IsKeyDown(Keys.LeftShift))
                camera.position -= camera.up * speed; //Down
        }

        if(camera.mouse_state != null) {
            camera.yaw += camera.mouse_state.Delta.X * sensitivity;
            camera.pitch -= camera.mouse_state.Delta.Y * sensitivity;

            if(camera.pitch > camera.pitch_clamp[0])
                camera.pitch = camera.pitch_clamp[0];
            else if(camera.pitch < camera.pitch_clamp[1])
                camera.pitch = camera.pitch_clamp[1];

            camera.direction.X = (float)(Math.Cos(MathHelper.DegreesToRadians(camera.pitch)) 
                                         * Math.Cos(MathHelper.DegreesToRadians(camera.yaw)));
            camera.direction.Y = (float)Math.Sin(MathHelper.DegreesToRadians(camera.pitch));
            camera.direction.Z = (float)(Math.Cos(MathHelper.DegreesToRadians(camera.pitch)) 
                                         * Math.Sin(MathHelper.DegreesToRadians(camera.yaw)));
            camera.direction = Vector3.Normalize(camera.direction);
        }

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        camera.right = Vector3.Normalize(Vector3.Cross(camera.direction, Vector3.UnitY));
        camera.up = Vector3.Normalize(Vector3.Cross(camera.right, camera.direction));

        transform.set(camera.position, camera.direction, camera.up);
    }
}
*/