using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace NetGL;

public struct Attitude {
    public Vector3 yaw_pitch_roll_degrees;

    public float yaw {
        get => yaw_pitch_roll_degrees.X;
        set => yaw_pitch_roll_degrees.X = wrap_yaw(value);
    }

    public float pitch {
        get => yaw_pitch_roll_degrees.Y;
        set => yaw_pitch_roll_degrees.Y = value;
    }

    public float roll {
        get => yaw_pitch_roll_degrees.Z;
        set => yaw_pitch_roll_degrees.Z = value;
    }

    private static readonly float degrees_to_radian = (float)(Math.PI / 180f);
    private static readonly float radians_to_degrees = (float)(180f / Math.PI);

    private static float wrap_yaw(float angle) {
        angle %= 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }

    public Attitude(float yaw, float pitch, float roll) {
        yaw_pitch_roll_degrees = new Vector3(wrap_yaw(yaw), pitch, roll);
    }

    public Attitude(in Vector3 forward, in float roll) {
        direction = forward;
        this.roll = roll;
    }

    public Vector3 direction {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var yaw_pitch_roll_rad = yaw_pitch_roll_degrees * degrees_to_radian;

            var v = new Vector3(
                    MathF.Cos(yaw_pitch_roll_rad.Y) * MathF.Cos(yaw_pitch_roll_rad.X),
                    MathF.Sin(yaw_pitch_roll_rad.Y),
                    MathF.Cos(yaw_pitch_roll_rad.Y) * MathF.Sin(yaw_pitch_roll_rad.X));
            /*
            Console.WriteLine(v);
            v.X = float.Clamp(v.X, -1e-6f, 1e6f);
            v.Y = float.Clamp(v.Y, -1e-6f, 1e6f);
            v.Z = float.Clamp(v.Z, -1e-6f, 1e6f);
            v.Normalize();
            Console.WriteLine(v);
            */

            return v.Normalized();
        }
        set {
            // Calculate yaw and pitch from the direction vector
            // Note: This is a simplified version and may need refinement
            yaw_pitch_roll_degrees[0] = MathF.Atan2(value.Z, value.X) * (180f / MathF.PI);
            yaw_pitch_roll_degrees[1] = MathF.Asin(value.Y) * (180f / MathF.PI);
            yaw_pitch_roll_degrees[2] = 0;
        }
    }

    public Vector2 spherical {
        get {
            var dir = direction;
            var r = MathF.Sqrt(dir.X * dir.X + dir.Y * dir.Y + dir.Z * dir.Z);
            var theta = MathF.Atan2(dir.Y, dir.X) * radians_to_degrees; // Azimuth
            var phi = MathF.Acos(dir.Z / r) * radians_to_degrees;       // Polar Angle
            return new Vector2(theta, phi);
        }
    }

    public float azimuth => spherical.X;
    public float polar => spherical.Y;

    public Vector3 up {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var roll_rad = yaw_pitch_roll_degrees.Z * degrees_to_radian;
            var basicUp = Vector3.Cross(right, direction).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicUp, rollQuaternion).Normalized();
        }
    }

    public Vector3 right {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            var roll_rad = yaw_pitch_roll_degrees.Z * degrees_to_radian;
            var basicRight = Vector3.Cross(direction, Vector3.UnitY).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicRight, rollQuaternion).Normalized();
        }
    }

    public override string ToString() => $"yaw: {yaw}, pitch: {pitch}, roll: {roll}";

    public Vector3 left => -right;
    public Vector3 down => -up;

    public static implicit operator Vector3(Attitude att) => new (att.yaw, att.pitch, att.roll);
    public static implicit operator Attitude(Vector3 vec) => new (vec.X, vec.Y, vec.Z);
}