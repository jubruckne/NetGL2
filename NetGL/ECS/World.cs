namespace NetGL.ECS;

public class World {
    private readonly List<Entity> entities = [];
    private readonly List<(System system, Entity[] entities)> systems = [];
    private readonly Dictionary<Type, Buffer> component_buffer = [];

    private System add_system(in System sys) {
        Console.Write($"adding system {sys} with filter: ");
        foreach (var t in sys.component_filter) {
            Console.Write($"{t.Name} ");
        }

        var ents = filter(sys.component_filter);
        systems.Add((sys, ents));
        
        Console.Write(" Entities: ");
        foreach (var e in ents) {
            Console.Write($"{e.name} ");
        }
        
        Console.WriteLine();

        return sys;
    }
    
    public Entity[] filter(Type[] components) {
        List<Entity> result = [];
        foreach (var ent in entities) {
            if (ent.supports(components)) result.Add(ent);
        }

        return result.ToArray();
    }

    public System create_system<S>() where S: System, new(){
        return add_system(new S());
    }

    public System create_system<C1>(string name, System<C1>.UpdateDelegate? update_action = null) where C1: struct, IComponent<C1> {
        return add_system(new System<C1>(name, update_action));
    }

    public System create_system<C1, C2>(string name, System<C1, C2>.UpdateDelegate? update_action = null) where C1: struct, IComponent<C1> where C2: struct, IComponent<C2> {
        return add_system(new System<C1, C2>(name, update_action));
    }
    
    public Entity create_entity(string name) {
        var e = new Entity(name);
        entities.Add(e);
        
        return e;
    }

    public Entity create_entity<T1>(string name, T1? comp = null) where T1: struct, IComponent<T1> {
        var entity = create_entity(name);
        
        var buffer = allocate_component_buffer<T1>();
        entity.components.Add(typeof(T1), new Entity.EntityComponent(buffer, buffer.append(comp ?? new T1())));

        return entity;
    }
    
    public Entity create_entity<T1, T2>(string name, T1? comp1 = null, T2? comp2 = null) where T1: struct, IComponent<T1> where T2: struct, IComponent<T2> {
        var entity = create_entity(name);

        var buf1 = allocate_component_buffer<T1>();
        entity.components.Add(typeof(T1), new Entity.EntityComponent(buf1, buf1.append(comp1 ?? new T1())));
        
        var buf2 = allocate_component_buffer<T2>();
        entity.components.Add(typeof(T2), new Entity.EntityComponent(buf2, buf2.append(comp2 ?? new T2())));

        return entity;
    }
    
    public Entity create_entity<T1, T2, T3>(string name, T1? comp1, T2? comp2, T3? comp3) where T1: struct, IComponent<T1> where T2: struct, IComponent<T1> where T3: struct, IComponent<T3> {
        var entity = create_entity(name);

        var buf1 = allocate_component_buffer<T1>();
        entity.components.Add(typeof(T1), new Entity.EntityComponent(buf1, buf1.append(comp1 ?? new T1())));
        
        var buf2 = allocate_component_buffer<T2>();
        entity.components.Add(typeof(T2), new Entity.EntityComponent(buf2, buf2.append(comp2 ?? new T2())));
        
        var buf3 = allocate_component_buffer<T3>();
        entity.components.Add(typeof(T3), new Entity.EntityComponent(buf3, buf3.append(comp3 ?? new T3())));

        return entity;
    }
    
    private Buffer<T> allocate_component_buffer<T>() where T : struct {
        if (!component_buffer.ContainsKey(typeof(T))) {
            Console.WriteLine($"making new buffer for component {typeof(T).Name}");
            component_buffer.Add(typeof(T), new ArrayBuffer<T>());
        }

        return (Buffer<T>)component_buffer[typeof(T)];
    }

    public void update_systems(in float delta_time) {
        foreach (var sys in systems) {
            sys.system.update(sys.entities, delta_time);
        }
    }
}