using System.Numerics;
using OpenTK.Mathematics;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL.ECS;

public readonly struct Range<T> where T: INumber<T> {
    private readonly T min;
    private readonly T max;

    public Range(T min, T max) {
        this.min = min;
        this.max = max;
    }

    public T clamp(in T value) => value;  //T.Clamp(value, min, max);
}

public struct Rotation {
    private Quaternion quat;

    public readonly Range<float> yaw_range = new Range<float>(-180, 180);
    public readonly Range<float> pitch_range = new Range<float>(-45, 45);
    public readonly Range<float> roll_range = new Range<float>(-180, 180);

    public Rotation(in Quaternion quaternion) => quat = quaternion;
    public Rotation(in Rotation rotation) => quat = rotation.quat;
    public Rotation(in Matrix4 matrix) => quat = Quaternion.FromMatrix(new Matrix3(matrix));
    public Rotation(in Matrix3 matrix) => quat = Quaternion.FromMatrix(matrix);

    public Rotation(Vector3 direction) {
        quat = Quaternion.Identity;
        quat *= Quaternion.FromAxisAngle(direction, 90f);
    }

    private const float degrees_to_radian = (float)(Math.PI / 180f);
    private const float radians_to_degrees = (float)(180f / Math.PI);

    public Vector3 yaw_pitch_roll {
        get => (
            quat.ToEulerAngles().X * radians_to_degrees,
            quat.ToEulerAngles().Y * radians_to_degrees,
            quat.ToEulerAngles().Z * radians_to_degrees
            );
        set => quat = Quaternion.FromEulerAngles(value * degrees_to_radian);
    }

    public void yaw(in float degrees) {
        quat *= Quaternion.FromEulerAngles(0, degrees * degrees_to_radian, 0);
    }

    public void pitch(in float degrees) {
        quat *= Quaternion.FromEulerAngles(degrees * degrees_to_radian, 0, 0);
    }

    public void roll(in float degrees) {
        quat *= Quaternion.FromAxisAngle(forward, degrees * degrees_to_radian).Normalized();
    }

    public Vector3 forward => Vector3.Transform(-Vector3.UnitZ, quat).Normalized();
    public Vector3 back => Vector3.Transform(Vector3.UnitZ, quat).Normalized();
    public Vector3 up => Vector3.Transform(Vector3.UnitY, quat).Normalized();
    public Vector3 down => Vector3.Transform(-Vector3.UnitY, quat).Normalized();
    public Vector3 right => Vector3.Transform(Vector3.UnitX, quat).Normalized();
    public Vector3 left => Vector3.Transform(-Vector3.UnitX, quat).Normalized();

    public override string ToString() {
        quat.ToAxisAngle(out var axis, out var angle);
        return $"{axis}, {angle.radians_to_degrees():F1} deg";
    }

    public static implicit operator Rotation(Quaternion quat) => new (quat);

    public static class Direction {
        public static Rotation Left => new Rotation(new Vector3(-1, 0, 0));
        public static Rotation Right => new Rotation(new Vector3(1, 0, 0));

        public static Rotation Up => new Rotation(new Vector3(0, 1, 0));
        public static Rotation Down => new Rotation(new Vector3(0, -1, 0));

        public static Rotation Back => new Rotation(new Vector3(0, 0, 1));
        public static Rotation Forward => new Rotation(new Vector3(0, 0, -1));
    }
}