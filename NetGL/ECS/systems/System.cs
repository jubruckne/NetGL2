namespace NetGL.ECS;

public abstract class System {
    public string name { get; }
    public Type[] component_filter { get; }

    protected internal System(string name, Type[] component_filter) {
        this.name = name;
        this.component_filter = component_filter;
    }

    public abstract void update(in Entity[] entities, in float delta_time);
}

public class System<C1>: System where C1 : struct, IComponent<C1> {
    public delegate void UpdateDelegate(ref C1 c1, in float delta_time);
    private readonly UpdateDelegate? on_update;
    
    protected internal System(string name, UpdateDelegate? on_update = null): base(name, [typeof(C1)]) {
        this.on_update = on_update;
    }

    public override void update(in Entity[] entities, in float delta_time) {
        foreach (Entity ent in entities) {
            on_update?.Invoke(ref ent.get<C1>(), delta_time);
        }
    }
}

public class System<C1, C2>: System where C1 : struct, IComponent<C1> where C2 : struct, IComponent<C2> {
    public delegate void UpdateDelegate(ref C1 c1, ref C2 c2, in float delta_time);
    private readonly UpdateDelegate? on_update;
    
    protected internal System(string name, UpdateDelegate? on_update = null): base(name, [typeof(C1), typeof(C2)]) {
        this.on_update = on_update;
    }
    
    public override void update(in Entity[] entities, in float delta_time) {
        foreach (var ent in entities) {
            on_update?.Invoke(ref ent.get<C1>(), ref ent.get<C2>(), delta_time);
        }
    }
}