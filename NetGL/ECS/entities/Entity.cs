namespace NetGL.ECS;

public class Entity {
    internal readonly struct EntityComponent {
        internal readonly Buffer buffer;
        internal readonly int index;

        internal EntityComponent(in Buffer buffer, in int index) {
            this.buffer = buffer;
            this.index = index;
        }
    }
    
    public string name { get; }
    
    internal Dictionary<Type, EntityComponent> components { get; }

    internal Entity(string name) {
        this.name = name;
        this.components = [];
    }

    public ref T get<T>() where T: struct, IComponent<T> {
        if(components.TryGetValue(typeof(T), out var component))
            return ref ((Buffer<T>)component.buffer)[component.index];

        throw new ArgumentOutOfRangeException(nameof(T));
    }

    public bool supports<C1>() where C1 : struct, IComponent<C1> {
        return supports(typeof(C1));
    }
    
    public bool supports<C1, C2>() where C1 : struct, IComponent<C1> where C2 : struct, IComponent<C2>{
        return supports(typeof(C1), typeof(C2));
    }
    
    public bool supports<C1, C2, C3>() where C1 : struct, IComponent<C1> where C2 : struct, IComponent<C2> where C3 : struct, IComponent<C3> {
        return supports(typeof(C1), typeof(C2), typeof(C3));
    }

    public bool supports(params Type[] types) {
        foreach (var t in types) {
            if(!components.ContainsKey(t)) return false;
        }

        return true;
    }

    public delegate void UpdateAction<T1>(ref T1 component);
    public delegate void UpdateAction<T1, T2>(ref T1 component1, ref T2 component2);
    public delegate void UpdateAction<T1, T2, T3>(ref T1 component1, ref T2 component2, ref T3 component3);

    public void update<T>(UpdateAction<T> action) where T: struct, IComponent<T> {
        action(ref get<T>());
    }
    
    public void update<T1, T2>(UpdateAction<T1, T2> action) where T1: struct, IComponent<T1> where T2: struct, IComponent<T2> {
        action(ref get<T1>(), ref get<T2>());
    }
}