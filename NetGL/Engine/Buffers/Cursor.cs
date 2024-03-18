namespace NetGL;

public static class Cursor {
    public static Cursor<T> get_cursor<T>(this Span<T> span) where T : unmanaged
        => new Cursor<T>(span);

    static void test() {
    }
}

public ref struct Cursor<T> where T: unmanaged {
    private Span<T> span;

    public T value {
        get => span[0];
        set => span[0] = value;
    }

    public Cursor(Span<T> span) => this.span = span;

    public void write(in T value) {
        span[0] = value;
        next();
    }

    private bool next() {
        if(span.Length > 0)
            span = span[1..];
        return span.Length > 0;
    }

    public static Cursor<T> operator++(Cursor<T> cursor) {
        cursor.next();
        return cursor;
    }

    public static implicit operator bool(in Cursor<T> cursor) => cursor.span.Length > 0;
    public static explicit operator T(in Cursor<T> cursor) => cursor.value;
}