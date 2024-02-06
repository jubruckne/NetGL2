using System.Runtime.InteropServices;

namespace NetGL.ECS;

public class AmbientLight: Light<AmbientLight.Data>, IComponent<AmbientLight> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data {
        public Color color;

        public Data(in Color color) {
            this.color = color;
        }

        public override string ToString() {
            return $"color: {color}";
        }
    }

    public AmbientLight(in Entity entity, in Color color): base(entity, new Data(color)) { }
}