using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL;

using System.Runtime.CompilerServices;

public readonly struct Frustum: IFormattable {
    public readonly Plane left_plane;
    public readonly Plane right_plane;
    public readonly Plane top_plane;
    public readonly Plane bottom_plane;
    public readonly Plane near_plane;
    public readonly Plane far_plane;

    public ReadOnlySpan<Plane> planes
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in left_plane), 6);

    public static Frustum from_matrix(in Matrix4 matrix) => new Frustum(matrix);

    private Frustum(in Matrix4 matrix) {
        left_plane = new Plane(
                               float3(
                                      matrix.M14 + matrix.M11,
                                      matrix.M24 + matrix.M21,
                                      matrix.M34 + matrix.M31
                                     ),
                               matrix.M44 + matrix.M41
                              );

        right_plane = new Plane(
                                float3(
                                       matrix.M14 - matrix.M11,
                                       matrix.M24 - matrix.M21,
                                       matrix.M34 - matrix.M31
                                      ),
                                matrix.M44 - matrix.M41
                               );

        top_plane = new Plane(
                              float3(
                                     matrix.M14 - matrix.M12,
                                     matrix.M24 - matrix.M22,
                                     matrix.M34 - matrix.M32
                                    ),
                              matrix.M44 - matrix.M42
                             );

        bottom_plane = new Plane(
                                 float3(
                                        matrix.M14 + matrix.M12,
                                        matrix.M24 + matrix.M22,
                                        matrix.M34 + matrix.M32
                                       ),
                                 matrix.M44 + matrix.M42
                                );

        near_plane = new Plane(
                               float3(
                                      matrix.M13,
                                      matrix.M23,
                                      matrix.M33
                                     ),
                               matrix.M43
                              );

        far_plane = new Plane(
                              float3(
                                     matrix.M14 - matrix.M13,
                                     matrix.M24 - matrix.M23,
                                     matrix.M34 - matrix.M33
                                    ),
                              matrix.M44 - matrix.M43
                             );
    }

    public float aspect_ratio => near_plane.normal.x / near_plane.normal.y;
    public float fov          => MathF.Atan(near_plane.normal.y) * 2;
    public float near         => near_plane.D;
    public float far          => far_plane.D;
    public float3 position    => near_plane.normal * near_plane.D;
    public float3 direction   => near_plane.normal;
    public float3 up          => top_plane.normal;
    public float3 down        => bottom_plane.normal;
    public float3 right       => right_plane.normal;
    public float3 left        => left_plane.normal;
    public float3 forward     => near_plane.normal;
    public float3 back        => far_plane.normal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool contains(float3 point)
        => left_plane.signed_distance(point) >= 0 &&
           right_plane.signed_distance(point) >= 0 &&
           top_plane.signed_distance(point) >= 0 &&
           bottom_plane.signed_distance(point) >= 0 &&
           near_plane.signed_distance(point) >= 0 &&
           far_plane.signed_distance(point) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool contains(Box box)
        => left_plane.intersects(box) &&
           right_plane.intersects(box) &&
           top_plane.intersects(box) &&
           bottom_plane.intersects(box) &&
           near_plane.intersects(box) &&
           far_plane.intersects(box);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool intersects(Box box)
        => left_plane.intersects(box) ||
           right_plane.intersects(box) ||
           top_plane.intersects(box) ||
           bottom_plane.intersects(box) ||
           near_plane.intersects(box) ||
           far_plane.intersects(box);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool intersects(Plane plane)
        => left_plane.intersects(plane) ||
           right_plane.intersects(plane) ||
           top_plane.intersects(plane) ||
           bottom_plane.intersects(plane) ||
           near_plane.intersects(plane) ||
           far_plane.intersects(plane);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool intersects(Ray ray)
        => left_plane.intersects(ray) ||
           right_plane.intersects(ray) ||
           top_plane.intersects(ray) ||
           bottom_plane.intersects(ray) ||
           near_plane.intersects(ray) ||
           far_plane.intersects(ray);

    public string ToString(string? format, IFormatProvider? formatProvider) {
        FormattableString formattable = $"near: {near}, far: {far}, position: {position}, direction: {direction}";
        return formattable.ToString(formatProvider);
    }

    public override string ToString() {
        return $"near: {near}, far: {far}, position: {position}, direction: {direction}";
    }
}