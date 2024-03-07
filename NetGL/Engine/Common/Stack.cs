namespace NetGL;

public static class Stacker {
    private static readonly Dictionary<Type, Stack<object>> items = new ();

    public static void push<T>(this T obj) where T:notnull {
        if(!items.TryGetValue(typeof(T), out var stack)) {
            stack = new Stack<object>();
            items.Add(typeof(T), stack);
        }

        stack.Push(obj);
    }

    public static T pop<T>(this T obj) where T:notnull {
        if(!items.TryGetValue(typeof(T), out var stack)) {
            throw new ArgumentException("unbalanced pop!", nameof(obj));
        }

        return (T)stack.Pop();
    }
}