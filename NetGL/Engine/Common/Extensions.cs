using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Half = System.Half;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public static class OpenTKExtensions {
    public static VertexAttribPointerType to_vertex_attribute_pointer_type<T>(this T d) where T: unmanaged {
        return d switch {
            float                       => VertexAttribPointerType.Float,
            byte                        => VertexAttribPointerType.UnsignedByte,
            short                       => VertexAttribPointerType.Short,
            double                      => VertexAttribPointerType.Double,
            int                         => VertexAttribPointerType.Int,
            ushort                      => VertexAttribPointerType.UnsignedShort,
            uint                        => VertexAttribPointerType.UnsignedInt,
            Half                        => VertexAttribPointerType.HalfFloat,
            sbyte                       => VertexAttribPointerType.Byte,
            System.Numerics.Vector3     => VertexAttribPointerType.Float,
            OpenTK.Mathematics.Vector3  => VertexAttribPointerType.Float,
            OpenTK.Mathematics.Vector3h => VertexAttribPointerType.HalfFloat,
            OpenTK.Mathematics.Vector3i => VertexAttribPointerType.Int,
            OpenTK.Mathematics.Vector3d => VertexAttribPointerType.Double,
            Vectors.vec3<float>      => VertexAttribPointerType.Float,
            Vectors.vec3<double>     => VertexAttribPointerType.Double,
            Vectors.vec3<Half>       => VertexAttribPointerType.HalfFloat,
            _                           => Error.type_conversion_error<T, VertexAttribPointerType>(d)
        };
    }
}


public static class TypeExtensions {
    public static string join_to_string<T>(this IEnumerable<T> list, string format = "", string separator = ", ") {
        StringBuilder sb = new();

        if (format == "") {
            sb.AppendJoin(separator, list);
        } else {
            format = "{0:" + format + "}";
            foreach (var value in list) {
                sb.AppendFormat(CultureInfo.InvariantCulture, format, value);
                sb.Append(separator);
            }

            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }

    public static string get_type_name<T>(this T t, bool with_generic_arguments = true) where T: notnull {
        if (t is Type tt) return get_type_name(tt);
        var type = t.GetType();
        if(type.IsGenericType) {
            if(!with_generic_arguments)
                return $"{type.Name[..type.Name.IndexOf('`')]}";

            var genericArguments = type.GetGenericArguments()
                .Select(static x => x.Name)
                .Aggregate(static (x1, x2) => $"{x1}, {x2}");

            var g = type.Name.IndexOf('`');

            return g > 0
                ? $"{type.Name[..type.Name.IndexOf('`')]}<{genericArguments}>"
                : $"{type.Name}<{genericArguments}>";
        }
        return type.Name;
    }

    public static string get_type_name(this Type type, bool with_generic_arguments = true) {
        if(type.IsGenericType) {
            if(!with_generic_arguments)
                return $"{type.Name[..type.Name.IndexOf('`')]}";

            var genericArguments = type.GetGenericArguments()
                .Select(static x => x.get_type_name())
                .Aggregate(static (x1, x2) => $"{x1}, {x2}");

            var g = type.Name.IndexOf('`');
            return g > 0
                ? $"{type.Name[..type.Name.IndexOf('`')]}<{genericArguments}>"
                : $"{type.Name}<{genericArguments}>";
        }
        return type.Name;
    }


    public static unsafe int size_of<T>(this T obj) where T : unmanaged {
        return sizeof(T);
    }

    public static PixelType to_pixel_type<T>(this T d) where T: unmanaged, INumber<T> {
        switch (d) {
            case byte:  return PixelType.Byte;
            case float: return PixelType.Float;
            case half:  return PixelType.HalfFloat;
            case int:   return PixelType.Int;
            default:
                Error.type_conversion_error<T, PixelType>(d);
                return PixelType.Byte;
        }
    }

    public static int length_of_opengl(this PixelFormat pixel_format) {
        return pixel_format switch {
            PixelFormat.Rgba => 4,
            PixelFormat.Bgra => 4,
            PixelFormat.Rgb => 3,
            PixelFormat.Red => 1,
            _ => throw new NotImplementedException($"OpenGL pixel format {pixel_format} not implemented")
        };
    }

    public static int size_of_opengl(this PixelType pixel_type, PixelFormat pixel_format)
        => pixel_type.size_of_opengl() * pixel_format.length_of_opengl();

    public static int size_of_opengl(this PixelType pixel_type) {
        return pixel_type switch {
            PixelType.UnsignedByte => 1,
            PixelType.Byte => 1,
            PixelType.UnsignedShort => 2,
            PixelType.Short => 2,
            PixelType.UnsignedInt => 4,
            PixelType.Int => 4,
            PixelType.HalfFloat => 2,
            PixelType.Float => 4,
            _ => throw new NotImplementedException($"OpenGL pixel type {pixel_type} not implemented")
        };
    }

    public static int std140_alignment(this Type field_type) {
        return field_type switch {
            _ when field_type == typeof(int) => 4,
            _ when field_type == typeof(uint) => 4,
            _ when field_type == typeof(float) => 4,
            _ when field_type == typeof(System.Numerics.Vector2) => 8,
            _ when field_type == typeof(OpenTK.Mathematics.Vector2) => 8,
            _ when field_type == typeof(OpenTK.Mathematics.Vector2i) => 8,
            _ when field_type == typeof(float2) => 8,
            _ when field_type == typeof(int2) => 8,
            _ when field_type == typeof(System.Numerics.Vector3) => 16,
            _ when field_type == typeof(OpenTK.Mathematics.Vector3) => 16,
            _ when field_type == typeof(float3) => 16,
            _ when field_type == typeof(int3) => 16,
            _ when field_type == typeof(System.Numerics.Vector4) => 16,
            _ when field_type == typeof(OpenTK.Mathematics.Vector4) => 16,
            _ when field_type == typeof(float4) => 16,
            _ when field_type == typeof(int4)   => 16,
            _ when field_type == typeof(OpenTK.Mathematics.Matrix4) => 16,
            _ when field_type == typeof(System.Numerics.Matrix4x4) => 16,
            _ => throw new NotImplementedException($"Alignment not defined for type {field_type}")
        };
    }

}

public static class RandomExt {
    public static T random<T>(this IReadOnlyList<T> list) {
        return list[Random.Shared.Next(list.Count)];
    }

    public static IEnumerable<T> random<T>(this IReadOnlyList<T> list, int count) {
        for(int i = 0; i < count; i++)
            yield return list[Random.Shared.Next(list.Count)];
    }
}

public static class VectorExt {
    public static List<Vector2i> adjecency(this in Vector2i center, int distance = 1, int scale = 1) {
        List<Vector2i> pointsExactlyNStepsAway = [];

        // Top and bottom horizontal lines
        for (int dx = -distance; dx <= distance; dx++) {
            pointsExactlyNStepsAway.Add(new Vector2i(center.X + dx * scale, center.Y + distance * scale)); // Top
            pointsExactlyNStepsAway.Add(new Vector2i(center.X + dx * scale, center.Y - distance * scale)); // Bottom
        }

        // Left and right vertical lines, excluding corners which are already added
        for (int dy = -distance + 1; dy <= distance - 1; dy++) {
            pointsExactlyNStepsAway.Add(new Vector2i(center.X + distance * scale, center.Y + dy * scale)); // Right
            pointsExactlyNStepsAway.Add(new Vector2i(center.X - distance * scale, center.Y + dy * scale)); // Left
        }

        return pointsExactlyNStepsAway;
    }

    public static void round(this ref Vector3 vector) {
        if (vector.X > 0 && vector.X < 1e-6f) vector.X = 0;
        if (vector.Y > 0 && vector.Y < 1e-6f) vector.Y = 0;
        if (vector.Z > 0 && vector.Z < 1e-6f) vector.Z = 0;

        if (vector.X < 0 && vector.X > -1e-6f) vector.X = -0;
        if (vector.Y < 0 && vector.Y > -1e-6f) vector.Y = -0;
        if (vector.Z < 0 && vector.Z > -1e-6f) vector.Z = -0;
    }

    public static ref Vector3 randomize(this ref Vector3 vector, float min, float max) {
        vector.X = Random.Shared.NextSingle() * (max - min) + min;
        vector.Y = Random.Shared.NextSingle() * (max - min) + min;
        vector.Z = Random.Shared.NextSingle() * (max - min) + min;
        return ref vector;
    }

    public static ref Vector3 add(this ref Vector3 vector, float x = 0f, float y = 0f, float z = 0f) {
        vector.X += x;
        vector.Y += y;
        vector.Z += z;
        return ref vector;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="direction">
    /// A direction vector of unit length.
    /// </param>
    /// <param name="azimuth">
    /// Azimuth: -180 to 180 degrees, where:
    /// - 0 degrees points directly ahead (towards the -Z direction in OpenGL).
    /// - -90 degrees to the left (towards the -X direction).
    /// - +90 degrees to the right (towards the +X direction).
    /// - +/-180 degrees directly behind (towards the +Z direction).
    /// </param>
    /// <param name="altitude">
    /// Altitude -90 to 90 degrees, where:
    /// - 0 degrees is on the horizon (parallel to the XZ plane).
    /// - +90 degrees is directly overhead (up along the Y axis).
    /// - -90 degrees is directly underfoot (down along the Y axis).
    /// </param>
    public static void set_azimuth_altitude(ref this Vector3 direction, float azimuth, float altitude) {
        if (azimuth > 180f) azimuth -= 360f;
        if (azimuth < -180f) azimuth += 360f;

        if (altitude > 90f || altitude < -90f)
            throw new ArgumentOutOfRangeException($"{nameof(altitude)}={altitude}", $"altitude out of range -90..90");

        double azimuthRadians = (azimuth - 90d) * Math.PI / 180d;
        double altitudeRadians = altitude * Math.PI / 180d;

        direction.X = (float)(Math.Cos(altitudeRadians) * Math.Cos(azimuthRadians));
        direction.Y = (float)(Math.Sin(altitudeRadians));

        // OpenGL forward direction is -Z, thus negating the sine component to align with this convention
        direction.Z = -(float)(Math.Cos(altitudeRadians) * Math.Sin(azimuthRadians));
    }

    /// <summary>
    /// Convert a direction vector to azimuth and altitude.
    /// </summary>
    /// <returns>
    /// Azimuth and altitude in degrees
    /// Azimuth range: -180 to 180, where 0 is directly ahead, -90 is left, +90 is right, and +/-180 is directly behind.
    /// Altitude range: -90 to 90, where 0 is on the horizon, +90 is directly overhead, and -90 is directly underfoot.
    /// </returns>
    public static (float azimuth, float altitude) to_azimuth_altitude(this Vector3 direction) {
        // Normalize the input direction vector to ensure it represents a direction only
        // and does not affect the calculation of angles with its magnitude.
        direction = Vector3.Normalize(direction);

        // Altitude Calculation:
        // Derived from the Y component of the direction vector, representing elevation/depression angle.
        // Math.Asin returns a value in radians, convert it to degrees.
        float altitude = (float)(Math.Asin(direction.Y) * (180.0 / Math.PI));

        // Azimuth Calculation:
        // Calculated from the X and Z components of the direction vector.
        // Math.Atan2 returns the angle in radians from the X-axis to the point (z, x), convert it to degrees.
        // Adjusting by adding 90 degrees to shift the reference so that 0 degrees points forward.
        float azimuth = (float)(Math.Atan2(-direction.Z, direction.X) * (180.0 / Math.PI) + 90.0);

        // Normalize the azimuth to the range of -180 to 180 degrees
        if (azimuth > 180) {
            azimuth -= 360;
        } else if (azimuth < -180) {
            azimuth += 360;
        }

        // Return the azimuth and altitude in degrees
        // Azimuth range: -180 to 180, where 0 is directly ahead, -90 is left, +90 is right, and +/-180 is directly behind.
        // Altitude range: -90 to 90, where 0 is on the horizon, +90 is directly overhead, and -90 is directly underfoot.
        return (azimuth, altitude);
    }

}

public static class ArrayExt {
    public static List<T> writeable<T>(this IReadOnlyList<T> list) => (List<T>)list;

    public static Dictionary<TKey, TValue> writeable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict)
        where TKey: notnull
        => (Dictionary<TKey, TValue>)dict;

    public static Map<TKey, TValue> writeable<TKey, TValue>(this ReadOnlyMap<TKey, TValue> dict)
        where TKey: notnull
        where TValue: notnull
        => (Map<TKey, TValue>)dict;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool can_reinterpret<TIn, TOut>(this TIn input) where TIn: unmanaged where TOut: unmanaged, INumberBase<TOut> {
        if (sizeof(TIn) != sizeof(TOut)) return false;
        if (input.is_float() && TOut.One.is_float()) return true;
        if (input.is_integer() && TOut.One.is_integer()) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool can_reinterpret<TIn, TOut>() where TIn: unmanaged, INumberBase<TIn> where TOut: unmanaged, INumberBase<TOut> {
        if (sizeof(TIn) != sizeof(TOut)) return false;
        if (TIn.One.is_float() && TOut.One.is_float()) return true;
        if (TIn.One.is_integer() && TOut.One.is_integer()) return true;
        return false;
    }

    public static ReadOnlySpan<TOut> convert_to<TOut>(this ReadOnlySpan<int> span) where TOut: unmanaged, INumberBase<TOut> {
        var output = new TOut[span.Length];

        for (var i = 0; i < span.Length; i++)
            output[i] = TOut.CreateChecked(span[i]);

        return output;
    }

    public static TOut convert_to<TOut>(this int value) where TOut: unmanaged, INumberBase<TOut> {
        return TOut.CreateChecked(value);
    }

    public static TOut convert_to<TOut>(this ushort value) where TOut: unmanaged, INumberBase<TOut> {
        return TOut.CreateChecked(value);
    }

    public static ReadOnlySpan<TOut> convert_to<TOut>(this ReadOnlySpan<ushort> span) where TOut: unmanaged, INumberBase<TOut> {
        var output = new TOut[span.Length];

        for (var i = 0; i < span.Length; i++)
            output[i] = TOut.CreateChecked(span[i]);

        return output;
    }

    public static ReadOnlySpan<TOut> convert_to<TOut>(this ReadOnlySpan<Vector3i> span) where TOut: unmanaged, INumberBase<TOut> {
        var output = new TOut[span.Length * 3];

        for (var i = 0; i < span.Length; i++) {
            output[i * 3] = TOut.CreateChecked(span[i].X);
            output[i * 3 + 1] = TOut.CreateChecked(span[i].Y);
            output[i * 3 + 2] = TOut.CreateChecked(span[i].Z);
        }

        return output;
    }

    public static ReadOnlySpan<TOut> convert_to<TOut>(this ReadOnlySpan<Index<int>> span) where TOut: unmanaged, INumberBase<TOut> {
        var output = new TOut[span.Length * 3];

        for (var i = 0; i < span.Length; i++) {
            output[i * 3] = TOut.CreateChecked(span[i].p0);
            output[i * 3 + 1] = TOut.CreateChecked(span[i].p1);
            output[i * 3 + 2] = TOut.CreateChecked(span[i].p2);
        }

        return output;
    }

    public static ReadOnlySpan<TOut> convert_to<TOut>(this ReadOnlySpan<Index<ushort>> span) where TOut: unmanaged, INumberBase<TOut> {
        var output = new TOut[span.Length * 3];

        for (var i = 0; i < span.Length; i++) {
            output[i * 3] = TOut.CreateChecked(span[i].p0);
            output[i * 3 + 1] = TOut.CreateChecked(span[i].p1);
            output[i * 3 + 2] = TOut.CreateChecked(span[i].p2);
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TIn, TOut>(this ReadOnlySpan<TIn> span) where TIn: unmanaged where TOut: unmanaged, INumberBase<TOut> {
        if (can_reinterpret<int, TOut>())
            return new ReadOnlySpan<TOut>(ref Unsafe.As<TIn, TOut>(ref MemoryMarshal.GetReference(span)));
        Error.type_conversion_error<TIn, TOut>(span[0]); // convert_to<TOut>(span);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> reinterpret_as<TIn, TOut>(this ReadOnlySpan<TIn> span) where TIn: unmanaged where TOut: unmanaged {
        return MemoryMarshal.Cast<TIn, TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TOut>(this ReadOnlySpan<int> span) where TOut: unmanaged, INumberBase<TOut> {
        return can_reinterpret<int, TOut>()
            ? new ReadOnlySpan<TOut>(ref Unsafe.As<int, TOut>(ref MemoryMarshal.GetReference(span)))
            : convert_to<TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TOut>(this ReadOnlySpan<ushort> span) where TOut: unmanaged, INumberBase<TOut> {
        return can_reinterpret<ushort, TOut>()
            ? new ReadOnlySpan<TOut>(ref Unsafe.As<ushort, TOut>(ref MemoryMarshal.GetReference(span)))
            : convert_to<TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TOut>(this ReadOnlySpan<Vector3i> span) where TOut: unmanaged, INumberBase<TOut> {
        return can_reinterpret<int, TOut>()
            ? new ReadOnlySpan<TOut>(ref Unsafe.As<Vector3i, TOut>(ref MemoryMarshal.GetReference(span)))
            : convert_to<TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TOut>(this ReadOnlySpan<Index<int>> span) where TOut: unmanaged, INumberBase<TOut> {
        return can_reinterpret<int, TOut>()
            ? new ReadOnlySpan<TOut>(ref Unsafe.As<Index<int>, TOut>(ref MemoryMarshal.GetReference(span)))
            : convert_to<TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TOut> cast_to<TOut>(this ReadOnlySpan<Index<ushort>> span) where TOut: unmanaged, INumberBase<TOut> {
        return can_reinterpret<ushort, TOut>()
            ? new ReadOnlySpan<TOut>(ref Unsafe.As<Index<ushort>, TOut>(ref MemoryMarshal.GetReference(span)))
            : convert_to<TOut>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> as_readonly_span<T>(this List<T> list) where T : unmanaged {
        return CollectionsMarshal.AsSpan(list);
    }

    /*
    public static T lookup<T>(this IEnumerable<T> list, Predicate<T> condition) {
        foreach(var item in list)
            if (condition(item))
                return item;

        Error.index_out_of_range(condition);
        return default;
    }*/

    public static bool peek<T>(this IList<T> list, [MaybeNullWhen(false)] out T item) {
        lock (list) {
            if (list.Count != 0) {
                item = list[0];
                return true;
            }
        }

        item = default;
        return false;
    }

    public static bool pop<T>(this IList<T> list, Predicate<T> condition, [MaybeNullWhen(false)] out T item) {
        lock (list) {
            for (int index = 0; index < list.Count; index++) {
                if (condition(list[index])) {
                    item = list[index];
                    list.RemoveAt(index);
                    return true;
                }
            }
        }

        item = default;
        return false;
    }

    public static bool pop<T>(this IList<T> list, [MaybeNullWhen(false)] out T item) {
        lock (list) {
            if (list.Count != 0) {
                item = list[0];
                list.RemoveAt(0);
                return true;
            }
        }

        item = default;
        return false;
    }

    public static IEnumerable<KeyValuePair<string, T>> StartingWith<T>(this Dictionary<string, T> dict, string key) {
        if (dict.Count == 0)
            return Enumerable.Empty<KeyValuePair<string, T>>();

        if (string.IsNullOrEmpty(key))
            return dict;

        var result = new List<KeyValuePair<string, T>>();
        foreach (var item in dict)
            if (item.Key.StartsWith(key))
                result.Add(item);

        return result;
    }

    public static string array_to_string<T>(this IEnumerable<T> array) {
        StringBuilder sb = new();

        foreach (var x in array)
            sb.AppendLine(x?.ToString());

        return sb.ToString();
    }

    public static void for_each<T>(this IEnumerable<T> array, Action<T> action) {
        foreach (var e in array)
            action(e);
    }

    public static int sum<T>(this IEnumerable<T> list, Func<T, int> sum_function) {
        var result = 0;
        foreach (var e in list)
            result += sum_function(e);
        return result;
    }

    public static int count<TList, T>(this TList list, Predicate<T> count_function) where T: unmanaged where TList: IEnumerable<T> {
        var result = 0;
        foreach (var e in list)
            if (count_function(e))
                ++result;

        return result;
    }

    public static int count<T>(this IEnumerable<T> list, Func<T, bool> count_function) {
        var result = 0;
        foreach (var e in list)
            if (count_function(e))
                ++result;

        return result;
    }

    public static int count<T>(this IEnumerable<T> list) => list.Count();
}