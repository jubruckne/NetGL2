using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class DirectionalLight: Light<DirectionalLight.Data>, IComponent<DirectionalLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Vector3 direction;
        public Color ambient;
        public Color diffuse;
        public Color specular;

        public Data(in Vector3 direction, in Color ambient, in Color diffuse, in Color specular) {
            this.direction = direction;
            this.direction.Normalize();
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
        }
    }

    public DirectionalLight(in Entity entity, in Vector3 direction, in Color ambient, in Color diffuse,
        in Color specular)
        : base(entity, new Data(direction, ambient, diffuse, specular)) { }
}