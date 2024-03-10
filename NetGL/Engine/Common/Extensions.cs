using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTK.Mathematics;

namespace NetGL;

public static class TypeExtensions {
    /// <summary>
    /// Returns the type name. If this is a generic type, appends
    /// the list of generic type arguments between angle brackets.
    /// (Does not account for embedded / inner generic arguments.)
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>System.String.</returns>
    public static string GetFormattedName(this Type type) {
        if(type.IsGenericType) {
            string genericArguments = type.GetGenericArguments()
                .Select(x => x.Name)
                .Aggregate((x1, x2) => $"{x1}, {x2}");
            return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                   + $"<{genericArguments}>";
        }
        return type.Name;
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

public static class AngleExt {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float wrap_degrees(float angle) {
        angle %= 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float degree_to_radians(this float degrees) =>
        MathHelper.DegreesToRadians(wrap_degrees(degrees));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float radians_to_degrees(this float radians) =>
        wrap_degrees(MathHelper.RadiansToDegrees(radians));
}

public static class ArrayExt {
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

    public static int sum<T>(this IEnumerable<T> array, Func<T, int> sum_function) {
        int result = 0;
        foreach (var e in array)
            result += sum_function(e);
        return result;
    }
}