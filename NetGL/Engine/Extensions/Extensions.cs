using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace NetGL;

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

public static class VectorExt {
    public static ref System.Numerics.Vector3 as_sys_num_ref(ref this Vector3 vector) {
        return ref MemoryMarshal.Cast<Vector3, System.Numerics.Vector3>(MemoryMarshal.CreateSpan(ref vector, 1))[0];
    }

    public static ref System.Numerics.Vector4 as_sys_num_ref(ref this Vector4 vector) {
        return ref MemoryMarshal.Cast<Vector4, System.Numerics.Vector4>(MemoryMarshal.CreateSpan(ref vector, 1))[0];
    }

}