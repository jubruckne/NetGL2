using OpenTK.Mathematics;

namespace NetGL;

using System.Runtime.CompilerServices;

public readonly struct Frustum {
    public readonly Plane left_plane;
    public readonly Plane right_plane;
    public readonly Plane top_plane;
    public readonly Plane bottom_plane;
    public readonly Plane near_plane;
    public readonly Plane far_plane;

    public Frustum(in Matrix4 matrix) {
        left_plane = new Plane(
                               (matrix.M14 + matrix.M11, matrix.M24 + matrix.M21, matrix.M34 + matrix.M31),
                               matrix.M44 + matrix.M41
                              );
        right_plane = new Plane(
                                (matrix.M14 - matrix.M11, matrix.M24 - matrix.M21, matrix.M34 - matrix.M31),
                                matrix.M44 - matrix.M41
                               );
        top_plane = new Plane(
                              (matrix.M14 - matrix.M12, matrix.M24 - matrix.M22, matrix.M34 - matrix.M32),
                              matrix.M44 - matrix.M42
                             );
        bottom_plane = new Plane(
                                 (matrix.M14 + matrix.M12, matrix.M24 + matrix.M22, matrix.M34 + matrix.M32),
                                 matrix.M44 + matrix.M42
                                );
        near_plane = new Plane((matrix.M13, matrix.M23, matrix.M33), matrix.M43);
        far_plane = new Plane(
                              (matrix.M14 - matrix.M13, matrix.M24 - matrix.M23, matrix.M34 - matrix.M33),
                              matrix.M44 - matrix.M43
                             );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool contains(in float3 point)
        => left_plane.signed_distance(point) >= 0 &&
           right_plane.signed_distance(point) >= 0 &&
           top_plane.signed_distance(point) >= 0 &&
           bottom_plane.signed_distance(point) >= 0 &&
           near_plane.signed_distance(point) >= 0 &&
           far_plane.signed_distance(point) >= 0;
}