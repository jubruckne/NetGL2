using System.Runtime.InteropServices;

namespace NetGL.ECS;

public class AmbientLight: Light<AmbientLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Color4i color;

        public override string ToString() {
            return $"color: {color}";
        }
    }

    public AmbientLight(in Entity entity, in Color4i color): base(entity) {
        data.color = color;
    }
}