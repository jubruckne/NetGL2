using NetGL.ECS;

namespace NetGL;

public abstract class Property: INamed {
    public string name { get; }
    public Type item_type => GetType();

    protected Property(string name) {
        this.name = name;
    }
}

public sealed class Property<T>: Property {
    private T? property_value;
    private readonly Action<T?>? on_changed;

    public Property(string name, in T? value = default, Action<T?>? on_changed = null): base(name) {
        property_value  = value;
        this.on_changed = on_changed;
    }

    public T? value {
        get => this.property_value;
        set {
            this.property_value = value;
            on_changed?.Invoke(value);
        }
    }

    public static implicit operator T(Property<T?> property) {
        if (property.value != null) return property.value;
        throw new InvalidOperationException("Property value is null");
    }
}

public sealed class ReadOnlyProperty<T>: Property where T: unmanaged {
    public ReadOnlyProperty(string name, in T value): base(name) {
        this.value  = value;
    }

    public T value { get; }

    public static implicit operator T(ReadOnlyProperty<T> property) => property.value;
}