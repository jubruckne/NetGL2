namespace NetGL;

public readonly ref struct Finally<T> {
    private readonly List<(Action<T>, T)> actions;

    public Finally() => actions = [];
    public Finally(in Action<T> action, T value) => actions = [(action, value)];

    public void add(in Action<T> action, T value) => actions.Add((action, value));

    public void keep() {
        actions.Clear();
    }

    public void reset() {
        foreach(var (action, value) in actions)
            action(value);

        actions.Clear();
    }

    public void Dispose() {
        reset();
    }
}