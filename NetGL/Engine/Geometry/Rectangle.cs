global using Rectangle = NetGL.Rectangle<float>;

using System.Numerics;
using NetGL.Vectors;

namespace NetGL;

public static class RectangleExt {
    public static T get_area<T>(this Rectangle<T> rectangle) where T: unmanaged, INumber<T>, IMinMaxValue<T>
        => rectangle.width * rectangle.height;

    public static T get_area<T>(this ReadOnlySpan<Rectangle<T>> rectangles) where T: unmanaged, INumber<T>, IMinMaxValue<T> {
        var result = T.CreateChecked(0);

        foreach (var rectangle in rectangles)
            result += rectangle.get_area();

        return result;
    }
}

public readonly struct Rectangle<T>: IComparable<Rectangle<T>>
    where T: unmanaged, INumber<T>, IMinMaxValue<T> {
    public readonly vec2<T> bottom_left;
    public readonly vec2<T> top_right;

    public vec2<T> top_left => new(top, left);
    public vec2<T> bottom_right => new(bottom, right);

    public T x => bottom_left.x;
    public T y => bottom_left.y;
    public T width => top_right.x - bottom_left.x;
    public T height => top_right.y - bottom_left.y;
    public T left => bottom_left.x;
    public T right => top_right.x;
    public T top => top_right.y;
    public T bottom => bottom_left.y;

    public vec2<T> center {
        get {
            // verify that center can be calculated without overflow
            var center = (bottom_left + top_right) / T.CreateSaturating(2);
            if((float2)center != (float2)(bottom_left + top_right) / 2f)
                throw new OverflowException("Center calculation overflowed");
            return center;
        }
    }

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
                result[i + j * columns] = new(x, y, width, height);
            }
        }

        return result;
    }

    public static Rectangle<T> centered_at(vec2<T> center, T size) {
        var half_size = size / T.CreateChecked(2);
        return new(center.x - half_size, center.y - half_size, size, size);
    }

    public static Rectangle<T> square(T x, T y, T size)
        => new(x, y, size, size);

    public static Rectangle<T> from_points(IEnumerable<vec2<T>> points) {
        var min = vec2(T.MaxValue);
        var max = vec2(T.MinValue);

        foreach (var point in points) {
            if (point.x < min.x)
                min.x = point.x;

            if (point.y < min.y)
                min.y = point.y;

            if (point.x > max.x)
                max.x = point.x;

            if (point.y > max.y)
                max.y = point.y;
        }

        if(min.x == T.MaxValue)
            throw new ArgumentException(nameof(points));

        return new Rectangle<T>(min, max);
    }

    public bool contains(vec2<T> point)
        => point.x >= bottom_left.x
            && point.x <= top_right.x
            && point.y >= bottom_left.y
            && point.y <= top_right.y;

    public bool contains(Rectangle<T> other)
        => other.bottom_left.x >= bottom_left.x
            && other.bottom_left.y >= bottom_left.y
            && other.top_right.x <= top_right.x
            && other.top_right.y <= top_right.y;

    public bool intersects(Rectangle<T> other)
        => bottom_left.x < other.top_right.x
            && top_right.x > other.bottom_left.x
            && bottom_left.y < other.top_right.y
            && top_right.y > other.bottom_left.y;

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