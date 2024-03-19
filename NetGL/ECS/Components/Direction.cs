using System.Runtime.CompilerServices;

namespace NetGL.ECS;

public static class Angle {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Radians radians(float radians) => new Radians(radians);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Degrees degrees(float degrees) => new Degrees(degrees);

    public readonly struct Radians {
        private readonly float value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Radians(float value) => this.value = value;

        // Converts Radians to Degrees.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Degrees(Radians r) => new Degrees((float)(r.value * (180.0 / Math.PI)));

        // Allows Radians to be instantiated with a double value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Radians(float value) => new Radians(value);

        public override string ToString() => $"{value} rad";
    }

    public readonly struct Degrees {
        private readonly float value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Degrees(float value) => this.value = value;

        // Converts Degrees to Radians.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Radians(Degrees d) => new Radians((float)(d.value * (Math.PI / 180.0)));

        // Allows Degrees to be instantiated with a double value.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Degrees(float value) => new Degrees(value);

        public override string ToString() => $"{value} deg";
    }
}