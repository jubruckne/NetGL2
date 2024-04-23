global using Rectangle = NetGL.Rectangle<float>;

using System.Numerics;
using NetGL.Vectors;

namespace NetGL;

public readonly struct Rectangle<T>: IComparable<Rectangle<T>> where T: unmanaged, INumber<T> {
    public readonly vec2<T> bottom_left;
    public readonly vec2<T> top_right;

    public T x => bottom_left.x;
    public T y => bottom_left.y;
    public T width => top_right.x - bottom_left.x;
    public T height => top_right.y - bottom_left.y;
    public T left => bottom_left.x;
    public T right => top_right.x;
    public T top => top_right.y;
    public T bottom => bottom_left.y;

    public vec2<T> center => (bottom_left + top_right) / T.CreateChecked(2);

    public Rectangle(T x, T y, T width, T height) {
        bottom_left = vec2(x, y);
        top_right = vec2(x + width, y + height);
    }

    public Rectangle(vec2<T> bottom_left, vec2<T> top_right) {
        this.bottom_left = bottom_left;
        this.top_right = top_right;
    }

    public Rectangle<T>[] split_by_size(T width, T height) {
        Debug.assert(
                     width > T.CreateChecked(0)
                     && height > T.CreateChecked(0)
                     && this.width >= width
                     && this.height >= height
                     && this.width % width == T.CreateChecked(0)
                     && this.height % height == T.CreateChecked(0)
                    );

        var columns = int.CreateChecked(this.width / width);
        var rows = int.CreateChecked(this.height / height);

        return split(columns, rows);
    }

    public Rectangle<T>[] split(int columns, int rows) {
        Debug.assert(
                     columns > 0
                     && rows > 0
                     && this.width % T.CreateChecked(columns) == T.CreateChecked(0)
                     && this.height % T.CreateChecked(rows) == T.CreateChecked(0)
                    );

        var result = new Rectangle<T>[columns * rows];
        var width = this.width / T.CreateChecked(columns);
        var height = this.height / T.CreateChecked(rows);

        for (var i = 0; i < columns; ++i) {
            for (var j = 0; j < rows; ++j) {
                var x = bottom_left.x + width * T.CreateChecked(i);
                var y = bottom_left.y + height * T.CreateChecked(j);
                result[i * rows + j] = new(x, y, width, height);
            }
        }

        return result;
    }

    public static Rectangle<T> centered_at(vec2<T> center, T size) {
        var half_size = size / T.CreateChecked(2);
        return new(center.x - half_size, center.y - half_size, size, size);
    }

    public static implicit operator Rectangle<T>((T x, T y, T width, T height) rect)
        => new(rect.x, rect.y, rect.width, rect.height);

    public static implicit operator Rectangle<T>((vec2<T> bottom_left, vec2<T> top_right) rect)
        => new(rect.bottom_left, rect.top_right);

    public static Rectangle<T> operator+(Rectangle<T> rectangle, vec2<T> offset)
        => new(rectangle.bottom_left + offset, rectangle.top_right + offset);

    public static Rectangle<T> operator-(Rectangle<T> rectangle, vec2<T> offset)
        => new(rectangle.bottom_left - offset, rectangle.top_right - offset);

    public static Rectangle<T> operator*(Rectangle<T> rectangle, T scale)
        => new(rectangle.bottom_left * scale, rectangle.top_right * scale);

    public static Rectangle<T> operator/(Rectangle<T> rectangle, T scale)
        => new(rectangle.bottom_left / scale, rectangle.top_right / scale);

    int IComparable<Rectangle<T>>.CompareTo(Rectangle<T> other)
        => bottom_left == other.bottom_left
            ? top_right.CompareTo(other.top_right)
            : bottom_left.CompareTo(other.bottom_left);

    public override int GetHashCode()
        => HashCode.Combine(bottom_left.GetHashCode(), top_right.GetHashCode());

    public override string ToString() => $"Rectangle[{bottom_left}:{top_right}]";
}