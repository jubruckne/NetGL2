using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace NetGL;

public struct Attitude {
    public Vector3 yaw_pitch_roll_degrees;

    public float yaw {
        get => yaw_pitch_roll_degrees.X;
        set => yaw_pitch_roll_degrees.X = value;
    }

    public float pitch {
        get => yaw_pitch_roll_degrees.Y;
        set => yaw_pitch_roll_degrees.Y = value;
    }

    public float roll {
        get => yaw_pitch_roll_degrees.Z;
        set => yaw_pitch_roll_degrees.Z = value;
    }

    private static readonly float degrees_to_radian = (float)(Math.PI / 180.0);

    public Attitude(float yaw, float pitch, float roll) {
        yaw_pitch_roll_degrees = new Vector3(yaw, pitch, roll);
    }

    public Vector3 direction {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var yaw_pitch_roll_rad = yaw_pitch_roll_degrees * degrees_to_radian;

            return new Vector3(
                    MathF.Cos(yaw_pitch_roll_rad.Y) * MathF.Cos(yaw_pitch_roll_rad.X),
                    MathF.Sin(yaw_pitch_roll_rad.Y),
                    MathF.Cos(yaw_pitch_roll_rad.Y) * MathF.Sin(yaw_pitch_roll_rad.X))
                .Normalized();
        }
    }

    public Vector3 up {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var roll_rad = yaw_pitch_roll_degrees.Z * degrees_to_radian;
            var basicUp = Vector3.Cross(right, direction).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicUp, rollQuaternion);
        }
    }

    public Vector3 right {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var roll_rad = yaw_pitch_roll_degrees.Z * degrees_to_radian;
            var basicRight = Vector3.Cross(direction, Vector3.UnitY).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicRight, rollQuaternion);
        }
    }

    public Vector3 left => -right;
    public Vector3 down => -up;

    public static implicit operator Vector3(Attitude att) => new (att.yaw, att.pitch, att.roll);
    public static implicit operator Attitude(Vector3 vec) => new (vec.X, vec.Y, vec.Z);
}