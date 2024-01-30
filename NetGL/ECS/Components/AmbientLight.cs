using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class AmbientLight: Light<AmbientLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Color4 color;

        public override string ToString() {
            return $"color: {color}";
        }
    }

    public AmbientLight(in Entity entity, in Color4 color): base(entity) {
        data.color = color;
    }
}