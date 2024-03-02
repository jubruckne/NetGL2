using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace NetGL;

public struct Rotation {
    private float yaw_radians;
    private float pitch_radians;
    private float roll_radians;

    private static readonly float degrees_to_radian = (float)(Math.PI / 180f);
    private static readonly float radians_to_degrees = (float)(180f / Math.PI);

    public Rotation(float yaw, float pitch, float roll) {
        yaw_radians = wrap_angle(yaw) * degrees_to_radian;
        pitch_radians = pitch * degrees_to_radian;
        roll_radians = roll * degrees_to_radian;
    }

    public Vector3 yaw_pitch_roll {
        get => new (yaw, pitch, roll);
        set => (yaw, pitch, roll) = value;
    }

    public float yaw {
        get => yaw_radians * radians_to_degrees;
        set => yaw_radians = wrap_angle(value) * degrees_to_radian;
    }

    public float pitch {
        get => pitch_radians * radians_to_degrees;
        set => pitch_radians = value * degrees_to_radian;
    }

    public float roll {
        get => roll_radians * radians_to_degrees;
        set => roll_radians = value * degrees_to_radian;
    }

    private static float wrap_angle(float angle) {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    public Vector3 forward {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var v = new Vector3(
                MathF.Sin(yaw_radians) * MathF.Cos(pitch_radians),
                MathF.Sin(pitch_radians),
                -MathF.Cos(yaw_radians) * MathF.Cos(pitch_radians));

            v.Normalize();
            v.round();
            return v;
        }
    }

    public Vector3 back => -forward;

    public Vector3 up {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var fw = forward;

            if (fw == Vector3.UnitY) {
                // Forward is (0, 1, 0), looking directly up.
                return Vector3.UnitZ;
            }

            if (fw == -Vector3.UnitY) {
                // Forward is (0, -1, 0), looking directly down.
                return -Vector3.UnitZ;
            }

            var basic_up = Vector3.Cross(right, fw).Normalized();
            var q = Quaternion.FromAxisAngle(fw, roll_radians);
            return Vector3.Transform(basic_up, q).Normalized();
        }
    }

    public Vector3 down => -up;

    public Vector3 right {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var fw = forward;

            if (fw == Vector3.UnitY) {
                // Forward is (0, 1, 0), looking directly up.
                return Vector3.UnitX;
            }

            if (fw == -Vector3.UnitY) {
                // Forward is (0, -1, 0), looking directly down.
                return -Vector3.UnitX;
            }

            var basic_right = Vector3.Cross(fw, Vector3.UnitY).Normalized();
            var q = Quaternion.FromAxisAngle(fw, roll_radians);
            return Vector3.Transform(basic_right, q).Normalized();
        }
    }

    public Vector3 left => -right;

    public static Rotation Forward => new Rotation(yaw:0, pitch:0, roll:0);
    public static Rotation Back => new Rotation(yaw:180, pitch:0, roll:0);
    public static Rotation Left => new Rotation(yaw:-90, pitch:0, roll:0);
    public static Rotation Right => new Rotation(yaw:90, pitch:0, roll:0);
    public static Rotation Up => new Rotation(yaw:0, pitch:90, roll:0);
    public static Rotation Down => new Rotation(yaw:0, pitch:-90, roll:0);

    public static bool operator ==(Rotation left, Rotation right) {
        return MathF.Abs(left.yaw_radians - right.yaw_radians) < 1e-6f &&
               MathF.Abs(left.pitch_radians - right.pitch_radians) < 1e-6f &&
               MathF.Abs(left.roll_radians - right.roll_radians) < 1e-6f;
    }

    public static bool operator !=(Rotation left, Rotation right) {
        return !(left == right);
    }

    public override string ToString() => $"yaw, pitch, roll:{(yaw, pitch, roll)}, radians:{(yaw_radians, pitch_radians, roll_radians)}";
}