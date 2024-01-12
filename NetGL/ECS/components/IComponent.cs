namespace NetGL.ECS;

public interface IComponent {}
public interface IComponent<T>: IComponent where T: struct, IComponent<T> { }

public interface IUpdateableComponent {
    void update(in Entity entity, in float delta_time);
}