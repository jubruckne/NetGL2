using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4_net = System.Numerics.Vector4;
using Vector3_net = System.Numerics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace NetGL;

public static class AngleExt {
    public static float degree_to_radians(this float degrees) =>
        MathHelper.DegreesToRadians(degrees);

    public static float radians_to_degrees(this float radians) =>
        MathHelper.RadiansToDegrees(radians);
}

public static class VectorExt {
    public static ref System.Numerics.Vector3 as_sys_num_ref(ref this Vector3 vector) {
        return ref MemoryMarshal.Cast<Vector3, System.Numerics.Vector3>(MemoryMarshal.CreateSpan(ref vector, 1))[0];
    }

    public static ref System.Numerics.Vector4 as_sys_num_ref(ref this Vector4 vector) {
        return ref MemoryMarshal.Cast<Vector4, System.Numerics.Vector4>(MemoryMarshal.CreateSpan(ref vector, 1))[0];
    }

    public static ref System.Numerics.Vector4 as_sys_num_ref(ref this Color4 color) {
        return ref MemoryMarshal.Cast<Color4, System.Numerics.Vector4>(MemoryMarshal.CreateSpan(ref color, 1))[0];
    }
}