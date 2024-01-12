namespace NetGL.ECS;

public abstract class System {
    public string name { get; }
    public Type[] component_filter { get; }

    protected internal System(string name, Type[] component_filter) {
        this.name = name;
        this.component_filter = component_filter;
    }

    public abstract void update(in Entity[] entities, in float delta_time);
    public abstract void render(in Entity[] entities);
}

public class System<C1>: System where C1 : struct, IComponent<C1> {
    public delegate void UpdateDelegate(ref C1 c1, in float delta_time);
    public delegate void RenderDelegate(ref C1 c1);

    private readonly UpdateDelegate? on_update;
    private readonly RenderDelegate? on_render;

    protected internal System(string name, UpdateDelegate? on_update = null, RenderDelegate? on_render = null): base(name, [typeof(C1)]) {
        this.on_update = on_update;
        this.on_render = on_render;
    }

    public override void update(in Entity[] entities, in float delta_time) {
        foreach (Entity ent in entities) {
            on_update?.Invoke(ref ent.get<C1>(), delta_time);
        }
    }

    public override void render(in Entity[] entities) {
        foreach (Entity ent in entities) {
            on_render?.Invoke(ref ent.get<C1>());
        }
    }
}

public class System<C1, C2>: System where C1 : struct, IComponent<C1> where C2 : struct, IComponent<C2> {
    public delegate void UpdateDelegate(ref C1 c1, ref C2 c2, in float delta_time);
    public delegate void RenderDelegate(ref C1 c1, ref C2 c2);

    private readonly UpdateDelegate? on_update;
    private readonly RenderDelegate? on_render;

    protected internal System(string name, UpdateDelegate? on_update = null, RenderDelegate? on_render = null): base(name, [typeof(C1), typeof(C2)]) {
        this.on_update = on_update;
        this.on_render = on_render;
    }
    
    public override void update(in Entity[] entities, in float delta_time) {
        foreach (var ent in entities) {
            on_update?.Invoke(ref ent.get<C1>(), ref ent.get<C2>(), delta_time);
        }
    }

    public override void render(in Entity[] entities) {
        foreach (var ent in entities) {
            on_render?.Invoke(ref ent.get<C1>(), ref ent.get<C2>());
        }
    }
}