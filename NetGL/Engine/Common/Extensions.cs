using System.Text;
using OpenTK.Mathematics;

namespace NetGL;

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

}

public static class AngleExt {
    public static float degree_to_radians(this float degrees) =>
        MathHelper.DegreesToRadians(degrees);

    public static float radians_to_degrees(this float radians) =>
        MathHelper.RadiansToDegrees(radians);
}

public static class ArrayExt {
    public static string array_to_string<T>(this IEnumerable<T> array) {
        StringBuilder sb = new();

        foreach (var x in array)
            sb.AppendLine(x.ToString());

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