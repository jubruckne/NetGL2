#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

namespace NetGL.Vectors;

public interface ivec;

public interface ivec<T>: ivec {
    Span<T> as_span();
}

public interface ivec2<T>: ivec<T>{
    T x { get; }
    T y { get; }
}

public interface ivec3<T>: ivec2<T> {
    T z { get; }
}

public interface ivec4<T>: ivec3<T> {
    T w { get; }
}