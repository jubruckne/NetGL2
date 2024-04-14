namespace NetGL;

public readonly struct Union<T1, T2> where T1: class where T2: class {
    public readonly object value;

    public Union(T1 value) {
        this.value = value;
    }

    public Union(T2 value) {
        this.value = value;
    }

    public bool type_of<T>() => value is T;

    public bool match<T>(out T value) {
        if (this.value is T v) {
            value = v;
            return true;
        }
        value = default!;
        return false;
    }

    public static implicit operator Union<T1, T2>(in T1 value) => new(value);
    public static implicit operator Union<T1, T2>(in T2 value) => new(value);

    public static implicit operator T1?(Union<T1, T2> value) => value.value as T1;
    public static implicit operator T2?(Union<T1, T2> value) => value.value as T2;
}