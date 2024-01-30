namespace NetGL.ECS;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

public class PointLight: Light<PointLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Vector3 position;
        public (Color4 ambient, Color4 diffuse, Color4 specular) color;
        public (float constant, float linear, float quadratic) attenuation;
        public float intensity;
    };

    public PointLight(in Entity entity, in Data data): base(entity) {
        this.data = data;
    }
}