namespace NetGL;

public class Property<T> where T: class? {
    private T _value;
    private readonly Action<T> on_changed;

    public T value {
        get => this._value;
        set {
            this._value = value;
            on_changed(value);
        }
    }

    public Property(in T value, Action<T> on_changed) {
        _value = value;
        this.on_changed = on_changed;
    }

    public static implicit operator T(Property<T> property) => property.value;
}