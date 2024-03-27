#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

namespace NetGL.Vectors;

public interface ivec {
    I[] get_array<I>();
}

public interface ivec<out T>: ivec {
    T[] array { get; }
}

public interface ivec2<out T>: ivec<T>{
    T x { get; }
    T y { get; }
}

public interface ivec3<out T>: ivec2<T> {
    T z { get; }
}

public interface ivec4<out T>: ivec3<T> {
    T w { get; }
}