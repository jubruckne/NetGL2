using OpenTK.Mathematics;

namespace NetGL.ECS;

public struct ColorComponent: IComponent<ColorComponent> {
    public Color4 color;

    public ColorComponent(Color4? color) {
        this.color = color ?? Color4.White;
    }
    
    public void set(Color4 value) {
        color = value;
    }
    
    public override string ToString() {
        return color.ToString();
    }
}