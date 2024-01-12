namespace NetGL.ECS;

public struct ParentComponent: IComponent<ParentComponent> {
    private Entity parent;
    
    public ParentComponent(Entity parent) {
        this.parent = parent;
    }

    public void set(Entity value) {
        parent = value;
    }

    public override string ToString() {
        return parent.ToString() ?? string.Empty;
    }
}