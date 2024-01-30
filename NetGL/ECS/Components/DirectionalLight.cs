using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class DirectionalLight: Light<DirectionalLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Vector3 direction;
        public Color4 ambient;
        public Color4 diffuse;
        public Color4 specular;

        public Data(in Vector3 direction, in Color4 ambient, in Color4 diffuse, in Color4 specular) {
            this.direction = direction;
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
        }
    }

    public DirectionalLight(in Entity entity, in Vector3 direction, in Color4 ambient, in Color4 diffuse, in Color4 specular): base(entity) {
        data = new Data(direction, ambient, diffuse, specular);
    }
}