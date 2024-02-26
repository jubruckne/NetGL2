using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public struct Rotation {
    private Quaternion quat;

    public Rotation(in Quaternion quaternion) => quat = quaternion;
    public Rotation(in Rotation rotation) => quat = rotation.quat;
    public Rotation(in Matrix4 matrix) => quat = Quaternion.FromMatrix(new Matrix3(matrix));
    public Rotation(in Matrix3 matrix) => quat = Quaternion.FromMatrix(matrix);
    public Rotation(Vector3 direction) => quat = new Quaternion(direction, 0);

    public (float yaw, float pitch, float roll) yaw_pitch_roll {
        get => (quat.ToEulerAngles().X, quat.ToEulerAngles().Y, quat.ToEulerAngles().Z);
        set => quat = Quaternion.FromEulerAngles(yaw: value.yaw, pitch: value.pitch, roll: value.roll);
    }

    public (Vector3 axis, float angle) axis_angle {
        get {
            quat.ToAxisAngle(out var axis, out var angle);
            return (axis, angle);
        }
        set => quat = Quaternion.FromAxisAngle(value.axis, value.angle);
    }

    public Vector3 direction {
        get => quat.Xyz;
        set => quat.Xyz = value;
    }

    public Vector3 up {
        get {
            var roll_rad = quat.W;
            var basicUp = Vector3.Cross(right, direction).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicUp, rollQuaternion).Normalized();
        }
    }

    public Vector3 right {
        get {
            var roll_rad = quat.W;
            var basicRight = Vector3.Cross(direction, Vector3.UnitY).Normalized();
            var rollQuaternion = Quaternion.FromAxisAngle(direction, roll_rad);
            return Vector3.Transform(basicRight, rollQuaternion).Normalized();
        }
    }

    public static Rotation from_yaw_pitch_roll(float yaw, float pitch, float roll) {
        var t = new Rotation();
        t.yaw_pitch_roll = (yaw, pitch, roll);
        return t;
    }

    public void randomize_yaw_pitch_roll(float min, float max) {
        quat.X = Random.Shared.NextSingle() * (max - min) + min;
        quat.Y = Random.Shared.NextSingle() * (max - min) + min;
        quat.W = Random.Shared.NextSingle() * (max - min) + min;
    }

    public static Rotation from_axis_angle(in Vector3 axis, float angle) {
        var t = new Rotation();
        t.axis_angle = (axis, angle);
        return t;
    }

    public static Rotation from_direction(in Vector3 direction) {
        var t = new Rotation();
        t.direction = direction;
        return t;
    }

    public static implicit operator Rotation(Vector3 direction) => new (direction);
    public static implicit operator Rotation(Quaternion quat) => new (quat);
}