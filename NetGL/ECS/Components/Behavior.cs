namespace NetGL.ECS;

public class Behavior: IComponent<Behavior>, IUpdatableComponent {
    public Entity entity { get; }
    public string name { get; }
    private readonly Predicate<Entity>? condition;
    private readonly Action<Entity> action;

    internal Behavior(Entity entity, string name, Predicate<Entity>? condition, Action<Entity> action) {
        this.entity = entity;
        this.name = name;
        this.action = action;
        this.condition = condition;
    }

    public bool enable_update { get; set; } = true;

    public void update(float delta_time) {
        if (condition == null || condition(entity)) action(entity);
    }
}

public static class BehaviorExt {
    public static Behavior add_behavior(this Entity entity, Action<Entity> action) {
        var behavior = new Behavior(entity, "Behavior", null, action);
        entity.add(behavior);
        return behavior;
    }

    public static Behavior add_behavior(this Entity entity, Predicate<Entity> condition, Action<Entity> action) {
        var behavior = new Behavior(entity, "Conditional Behavior", condition, action);
        entity.add(behavior);
        return behavior;
    }
}