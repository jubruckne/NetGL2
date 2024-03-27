#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

global using half = System.Half;

namespace NetGL;

using System.Runtime.CompilerServices;
using System.Numerics;

public static class Numbers {
    public static bool between<T>(this T t, in T lower, in T upper)
        where T: INumber<T> => t > lower && t < upper;

    public static T sqrt<T>(this T number)
        where T: IRootFunctions<T>
        => T.Sqrt(number);

    public static T power<T>(this T value, in T exp)
        where T: IPowerFunctions<T>
        => T.Pow(value, exp);

    public static T clamp<T>(this T value, in T min, in T max) where T: INumber<T> {
        Debug.assert(min < max);

        if (value < min)
            return min;

        return value > max ? max : value;
    }

    public static int round(this float n) => (int)float.Round(n);
    public static int round(this double n) => (int)double.Round(n);
    public static int round(this half n) => (int)half.Round(n);
    public static T round<T>(this T t, in int digits) where T: IFloatingPoint<T> => T.Round(t, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float wrap_degrees(float angle) {
        angle %= 360;
        if (angle > 180) angle  -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float degree_to_radians(this float degrees) => degrees * ((float) Math.PI / 180f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float radians_to_degrees(this float radians) => wrap_degrees(radians * 57.295776f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool is_float<T>()
        where T: unmanaged
        => typeof(T) == typeof(float)
               || typeof(T) == typeof(double)
               || typeof(T) == typeof(OpenTK.Mathematics.Half)
               || typeof(T) == typeof(half);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool is_integer<T>()
        where T: unmanaged
        => typeof(T) == typeof(int)
               || typeof(T) == typeof(uint)
               || typeof(T) == typeof(byte)
               || typeof(T) == typeof(sbyte)
               || typeof(T) == typeof(short)
               || typeof(T) == typeof(ushort)
               || typeof(T) == typeof(long)
               || typeof(T) == typeof(ulong)
               || typeof(T) == typeof(nint)
               || typeof(T) == typeof(nuint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool is_float<T>(this T t)
        where T: unmanaged
        => t is float or double or OpenTK.Mathematics.Half or half;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool is_integer<T>(this T t)
        where T: unmanaged
        => t is int or uint or byte or sbyte or short or ushort or byte or long or ulong or nint or nuint;
}