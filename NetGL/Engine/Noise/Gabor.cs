using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using BulletSharp;

namespace NetGL;

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

public class GaborNoise {
    private readonly Vector128<float> aVec;
    private readonly Vector128<float> F0Vec;
    private readonly Vector128<float> cosOmega0;
    private readonly Vector128<float> sinOmega0;

    public GaborNoise() {
        double a      = 0.05;        // Gaussian envelope standard deviation
        double F0     = 0.0625;      // Frequency of the sinusoid
        double omega0 = Math.PI / 4; // Orientation of the sinusoid

        aVec      = Vector128.Create((float)(Math.PI * Math.PI * a * a));
        F0Vec     = Vector128.Create((float)(2 * Math.PI * F0));
        cosOmega0 = Vector128.Create((float)Math.Cos(omega0));
        sinOmega0 = Vector128.Create((float)Math.Sin(omega0));
    }

    private static readonly Vector128<float> two = Vector128.Create(0.5f);

    public Vector128<float> GaborKernel(Vector128<float> x, Vector128<float> y) {
        // Calculate xPrime and yPrime using FMA
        Vector128<float> xPrime = AdvSimd.FusedMultiplyAdd(x, cosOmega0, AdvSimd.Multiply(y, sinOmega0));
        Vector128<float> yPrime = AdvSimd.FusedMultiplyAdd(
                                                           x,
                                                           AdvSimd.Negate(sinOmega0),
                                                           AdvSimd.Multiply(y, cosOmega0)
                                                          );

        Vector128<float> xSquared = AdvSimd.Multiply(xPrime, xPrime);
        Vector128<float> ySquared = AdvSimd.Multiply(yPrime, yPrime);
        Vector128<float> sumXY    = AdvSimd.Add(xSquared, ySquared);

        // Using Fused Multiply-Add to calculate gExp = -π²a² * (x'² + y'²)
        Vector128<float> gExp = AdvSimd.FusedMultiplyAdd(aVec, sumXY, Vector128<float>.Zero);
        gExp = AdvSimd.Negate(gExp); // Negate since we need e^(-value)
        Vector128<float> gaussian   = Exp(gExp);
        Vector128<float> sinusoidal = Sin(AdvSimd.Multiply(F0Vec, xPrime)); // Assuming an approximation

        return AdvSimd.Multiply(gaussian, sinusoidal);
    }

    private static Vector128<float> Exp(Vector128<float> value) {
        // Using the first three terms of the Taylor series for e^-x
        // e^-x ≈ 1 - x + x^2 / 2 for small x
        var xSquared        = AdvSimd.Multiply(value, value);
        var xSquaredOverTwo = AdvSimd.Multiply(xSquared, two);
        // 1 - x + x^2 / 2
        return AdvSimd.Add(AdvSimd.Subtract(Vector128<float>.One, value), xSquaredOverTwo);
    }

    private static readonly Vector128<float> twoPi = Vector128.Create<float>(6.283185307179586477f);
    private static readonly Vector128<float> pi = Vector128.Create<float>(3.141592653589793239f);
    private static readonly Vector128<float> piOver2 = Vector128.Create<float>(1.570796326794896619f);
    private static readonly Vector128<float> piOver2x3 = Vector128.Create<float>((float)(3 * 1.570796326794896619));

    /// <summary>
    /// Computes an approximation of sine. Maximum error a little below 5e-7 for the interval -2 * Pi to 2 * Pi. Values further from the interval near zero have gracefully degrading error.
    /// </summary>
    /// <param name="x">Value to take the sine of.</param>
    /// <returns>Approximate sine of the input value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> Sin(Vector128<float> x) {
        //Similar to cos, use a rational approximation for the region of sin from [0, pi/2]. Use symmetry to cover the rest.
        //This has its own implementation rather than just calling into Cos because we want maximum fidelity near 0.
        var periodCount = x * 0.5f / MathF.PI;
        var periodFraction =
            periodCount - Vector128.Floor(periodCount); //This is a source of error as you get away from 0.
        var periodX = periodFraction * twoPi;
        //[0, pi/2] = f(x)
        //(pi/2, pi] = f(pi - x)
        //(pi, 3/2 * pi] = -f(x - pi)
        //(3/2 * pi, 2*pi] = -f(2 * pi - x)
        var y            = Vector128.ConditionalSelect(Vector128.GreaterThan(periodX, piOver2), pi - periodX, periodX);
        var inSecondHalf = Vector128.GreaterThan(periodX, pi);
        y = Vector128.ConditionalSelect(inSecondHalf, periodX - pi, y);
        y = Vector128.ConditionalSelect(Vector128.GreaterThan(periodX, piOver2x3), twoPi - periodX, y);

        //Using a fifth degree numerator and denominator.
        //This will be precise beyond a single's useful representation most of the time, but we're not *that* worried about performance here.
        //TODO: FMA could help here, primarily in terms of precision.
        //var y2 = y * y;
        //var y3 = y2 * y;
        //var y4 = y2 * y2;
        //var y5 = y3 * y2;
        //var numerator = 1.0000000015146604f * y + 0.06174562337697123f * y2 - 0.13993701695343166f * y3 - 0.006685815219853882f * y4 + 0.0040507708755727605f * y5;
        //var denominator = Vector<float>.One + 0.061745651499203795f * y + 0.02672943625500751f * y2 + 0.003606014457152456f * y3 + 0.0001700784176413186f * y4 + 0.00009018370615921334f * y5;
        var numerator =
            ((((0.0040507708755727605f * y - Vector128.Create<float>(0.006685815219853882f)) * y
               - Vector128.Create<float>(0.13993701695343166f)) * y + Vector128.Create<float>(0.06174562337697123f)) * y
             + Vector128.Create<float>(1.00000000151466040f)) * y;

        var denominator =
            ((((Vector128.Create<float>(0.00009018370615921334f) * y + Vector128.Create<float>(0.0001700784176413186f))
               * y
               + Vector128.Create<float>(0.003606014457152456f)) * y + Vector128.Create<float>(0.02672943625500751f))
             * y
             + Vector128.Create<float>(0.061745651499203795f)) * y + Vector128<float>.One;

        var result = numerator / denominator;
        return Vector128.ConditionalSelect(inSecondHalf, -result, result);
    }

    public static int is_aligned<T>(ref T v, int alignment) where T: unmanaged {
        unsafe {
            fixed (T* addr = &v) {
                return (int)((nint)addr % alignment);
            }
        }
    }

    public static void Cos_aligned(Span<float> data) {
        Debug.assert_equal(data.Length % Vector128<float>.Count, 0);
        Debug.assert_equal(is_aligned(ref data[0], 16), 0);

        var span = MemoryMarshal.Cast<float, Vector128<float>>(data);

        for (var i = 0; i < span.Length; ++i)
            span[i] = Cos(span[i]);
    }

    public static unsafe void Cos(Span<float> data) {
        Console.WriteLine("**** sizeof(Vector128<float>): " + sizeof(Vector128<float>));

        var unaligned_length = is_aligned(ref data[0], sizeof(Vector128<float>)) / sizeof(float);
        var vectorized_length = (data.Length - unaligned_length) / sizeof(Vector128<float>) * sizeof(float);

        Console.WriteLine("**** unaligned length: " + unaligned_length);

        for (var i = 0; i < unaligned_length ; ++i)
            data[i] = MathF.Cos(data[i]);

        Cos_aligned(data.Slice(unaligned_length, vectorized_length));

        for (var i = unaligned_length + vectorized_length; i < data.Length ; ++i)
            data[i] = MathF.Cos(data[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Cos(Vector128<float> x) {
        //Rational approximation over [0, pi/2], use symmetry for the rest.
        var periodCount = x * (0.5f / MathF.PI);
        var periodFraction =
            periodCount - Vector128.Floor(periodCount); //This is a source of error as you get away from 0.
        var periodX = periodFraction * twoPi;

        //[0, pi/2] = f(x)
        //(pi/2, pi] = -f(Pi - x)
        //(pi, 3 * pi / 2] = -f(x - Pi)
        //(3*pi/2, 2*pi] = f(2 * Pi - x)
        var y = Vector128.ConditionalSelect(Vector128.GreaterThan(periodX, piOver2), pi - periodX, periodX);
        y = Vector128.ConditionalSelect(Vector128.GreaterThan(periodX, pi), periodX - pi, y);
        y = Vector128.ConditionalSelect(Vector128.GreaterThan(periodX, piOver2x3), twoPi - periodX, y);

        //Using a fifth degree numerator and denominator.
        //This will be precise beyond a single's useful representation most of the time, but we're not *that* worried about performance here.
        //TODO: FMA could help here, primarily in terms of precision.
        //var y2 = y * y;
        //var y3 = y2 * y;
        //var y4 = y2 * y2;
        //var y5 = y3 * y2;
        //var numerator = Vector<float>.One - 0.15082367674208508f * y - 0.4578088075324152f * y2 + 0.06955843390178032f * y3 + 0.021317031205957775f * y4 - 0.003436308368583229f * y5;
        //var denominator = Vector<float>.One - 0.15082367538305258f * y + 0.04219116713777847f * y2 - 0.00585321045829395f * y3 + 0.0007451378206294365f * y4 - 0.00007650398834677185f * y5;
        var numerator =
            ((((Vector128.Create<float>(-0.003436308368583229f) * y + Vector128.Create<float>(0.021317031205957775f))
                  * y + Vector128.Create<float>(0.06955843390178032f)) * y
              - Vector128.Create<float>(0.4578088075324152f))
                * y - Vector128.Create<float>(0.15082367674208508f)) * y + Vector128<float>.One;
        var denominator =
            ((((Vector128.Create<float>(-0.00007650398834677185f) * y + Vector128.Create<float>(0.0007451378206294365f))
                  * y - Vector128.Create<float>(0.00585321045829395f)) * y
              + Vector128.Create<float>(0.04219116713777847f)) * y - Vector128.Create<float>(0.15082367538305258f)) * y
            + Vector128<float>.One;
        var result = numerator / denominator;
        return Vector128.ConditionalSelect(
                                           Vector128.BitwiseAnd(
                                                                Vector128.GreaterThan(periodX, piOver2),
                                                                Vector128.LessThan(periodX, piOver2x3)
                                                               ),
                                           -result,
                                           result
                                          );
    }
}