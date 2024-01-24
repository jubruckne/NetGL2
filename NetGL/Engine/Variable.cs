namespace NetGL;

public class Variable<T> where T: IEquatable<T> {
    private T current;
    private T previous;

    public Variable(T value) {
        current = value;
        previous = value;
    }

    public T value {
        get => current;
        set => current = value;
    }

    internal void flip() => previous = current;

    public bool has_changed() => current.Equals(previous);
}