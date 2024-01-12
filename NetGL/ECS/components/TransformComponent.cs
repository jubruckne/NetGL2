using OpenTK.Mathematics;

namespace NetGL.ECS;

public struct TransformComponent: IComponent<TransformComponent> {
    public Matrix4 transform;
    
    public TransformComponent(Matrix4? transform) {
        this.transform = transform ?? new Matrix4();
    }

    public void set(Matrix4 value) {
        transform = value;
    }

    public override string ToString() {
        return transform.ToString();
    }
}
