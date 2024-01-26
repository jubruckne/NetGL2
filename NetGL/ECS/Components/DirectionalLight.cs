using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class DirectionalLight: Light<DirectionalLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Vector3 direction;
        public (Color4i ambient, Color4i diffuse, Color4i specular) color;
    }

    public DirectionalLight(in Entity entity, in Data data): base(entity) {
        this.data = data;
    }
}