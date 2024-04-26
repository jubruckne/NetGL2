global using Quad = NetGL.Quad<float>;

using System.Numerics;
using NetGL.Vectors;

namespace NetGL;

public readonly struct Quad<T>: IComparable<Quad<T>> where T: unmanaged, INumber<T> {
    public readonly vec3<T> min;
    public readonly vec3<T> max;

    public T width => max.x - min.x;
    public T height => max.y - min.y;
    public T depth => max.z - min.z;

    public T left => min.x;
    public T right => max.x;
    public T top => max.y;
    public T bottom => min.y;
    public T front => min.z;
    public T back => max.z;

    public vec3<T> center => (min + max) / T.CreateChecked(2);

    public Quad(T x, T y, T z, T width, T height, T depth) {
        min = vec3(x, y, z);
        max = vec3(x + width, y + height, z + depth);
    }

    public Quad(vec3<T> min, vec3<T> max) {
        this.min = min;
        this.max = max;
    }

    public static Quad<T> centered_at(vec3<T> center, T size) {
        var half_size = vec3<T>(size / T.CreateChecked(2), size / T.CreateChecked(2), size / T.CreateChecked(2));
        return new(center - half_size, center + half_size);
    }

    public static implicit operator Quad<T>((T x, T y, T z, T width, T height, T depth) rect)
        => new(rect.x, rect.y, rect.z, rect.width, rect.height, rect.depth);

    public static implicit operator Quad<T>((vec3<T> min, vec3<T> max) rect)
        => new(rect.min, rect.max);

    public static Quad<T> operator+(Quad<T> quad, vec3<T> offset)
        => new(quad.min + offset, quad.max + offset);

    public static Quad<T> operator-(Quad<T> quad, vec3<T> offset)
        => new(quad.min - offset, quad.max - offset);

    public static Quad<T> operator*(Quad<T> quad, T scale)
        => new(quad.min * scale, quad.max * scale);

    public static Quad<T> operator/(Quad<T> quad, T scale)
        => new(quad.min / scale, quad.max / scale);

    int IComparable<Quad<T>>.CompareTo(Quad<T> other)
        => min == other.min ? max.CompareTo(other.max) : min.CompareTo(other.min);

    public override int GetHashCode()
        => HashCode.Combine(min.GetHashCode(), max.GetHashCode());

    public override string ToString() => $"Quad[{min}:{max}]";
}