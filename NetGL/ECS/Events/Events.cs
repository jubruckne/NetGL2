namespace NetGL.ECS.Events;

public interface IEvent: INamed {
}

public readonly struct EntityComponentAdded: IEvent {
    public readonly Entity entity;
    public readonly IComponent component;

    public EntityComponentAdded(Entity entity, IComponent component) {
        this.entity = entity;
        this.component = component;
    }

    public string name => $"{entity.name}.{component.name}";
}

public interface IEventSender<in TSource, in TEvent> where TSource: class where TEvent: IEvent {
    void send_event(TSource source, TEvent data);
}

public interface IEventReceiver<TEvent> where TEvent: IEvent {
    public delegate void Delegate(in TEvent data);

    Delegate on_event { get; }
}