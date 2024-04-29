using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL.Vectors;

public partial struct vec4<T>:
    ivec4<T> ,
    IEquatable<vec4<T>>
    where T: unmanaged, INumber<T> {

    public T x;
    public T y;
    public T z;
    public T w;

    public vec4(vec3<T> xyz, T w) {
        this.x = xyz.x;
        this.y = xyz.y;
        this.z = xyz.z;
        this.w = w;
    }

    public vec4(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void set(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec4<T> set<P>(P x, P y, P z, P w)
        where P: unmanaged, INumberBase<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        this.z = T.CreateSaturating(z);
        this.w = T.CreateSaturating(w);
        return this;
    }

    public static implicit operator vec4<T>((T x, T y, T z, T w) vector) =>
        new vec4<T>(vector.x, vector.y, vector.z, vector.w);

    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
    T ivec3<T>.z => z;
    T ivec4<T>.w => w;
}

public partial struct vec4<T> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(vec4<T> other)
        => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> as_span()
        => MemoryMarshal.CreateSpan(ref x, 4);

    public override bool Equals(object? obj)
        => obj is vec3<T> other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(x, y, z, w);

    public override string ToString() => $"({x}, {y}, {z}, {w})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T x, out T y, out T z, out T w) {
        x = this.x;
        y = this.y;
        z = this.z;
        w = this.w;
    }
}