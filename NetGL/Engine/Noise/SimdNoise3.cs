using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace NetGL.experiment;

[SkipLocalsInit]
public class SimdNoise3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> permute(Vector128<float> x) {
        var factor = Vector128.Create(34.0f);
        var one = Vector128<float>.One;
        var mod    = Vector128.Create(289.0f);

        // (x * 34.0 + 1.0) * x
        x = AdvSimd.FusedMultiplyAdd(x, factor, one);
        x = AdvSimd.Multiply(x, x);

        // Return x mod 289 without using division
        // x - floor(x / mod) * mod
        var reciprocal = Vector128.Create(1.0f / 289.0f);
        var scaled     = AdvSimd.Multiply(x, reciprocal);
        var floored    = AdvSimd.Floor(scaled);
        var product    = AdvSimd.Multiply(floored, mod);
        return AdvSimd.Subtract(x, product);
    }

    public static Vector128<float> Snoise(Vector128<float> x, Vector128<float> y) {
        var C1 = Vector128.Create(0.211324865405187f);
        var C2 = Vector128.Create(0.366025403784439f);
        var C3 = Vector128.Create(-0.577350269189626f);
        var C4 = Vector128.Create(0.024390243902439f);

        var Cxx = Vector128.Create(C1.GetElement(0), C2.GetElement(0), C3.GetElement(0), C4.GetElement(0));
        var Cyy = Vector128.Create(C2.GetElement(0), C2.GetElement(0), C2.GetElement(0), C2.GetElement(0));

        var xy = AdvSimd.Add(x, AdvSimd.Multiply(y, Cyy));
        var i = AdvSimd.Floor(xy);
        var x0 = AdvSimd.Subtract(xy, AdvSimd.Add(i, AdvSimd.Multiply(i, Cxx)));

        var mask = AdvSimd.CompareGreaterThan(x0, y);
        var i1 = AdvSimd.BitwiseSelect(mask, Vector128.Create(1.0f, 0.0f, 0.0f, 0.0f), Vector128.Create(0.0f, 1.0f, 0.0f, 0.0f));

        var x12 = AdvSimd.Add(x0, Vector128.Create(C1.GetElement(0), C2.GetElement(0), C3.GetElement(0), C4.GetElement(0)));
        x12 = AdvSimd.Subtract(x12, i1);

        var mod        = Vector128.Create(289.0f);
        var reciprocal = Vector128.Create(1.0f / 289.0f);
        i = AdvSimd.Subtract(i, AdvSimd.Multiply(AdvSimd.Floor(AdvSimd.Multiply(i, reciprocal)), mod));

        var p = permute(permute(AdvSimd.Add(i, Vector128.Create(0.0f, i1.GetElement(1), 1.0f, 0.0f))) +
                        AdvSimd.Add(i, Vector128.Create(0.0f, i1.GetElement(0), 1.0f, 0.0f)));

        var m = AdvSimd.Max(Vector128.Create(0.5f), AdvSimd.Subtract(Vector128.Create(0.5f), AdvSimd.Multiply(x0, x0)));
        m = AdvSimd.Max(m, AdvSimd.Subtract(Vector128.Create(0.5f), AdvSimd.Multiply(x12, x12)));

        m = AdvSimd.Multiply(m, m);
        m = AdvSimd.Multiply(m, m);

        var Cwww    = Vector128.Create(C4.GetElement(0), C4.GetElement(0), C4.GetElement(0), C4.GetElement(0));
        var x_fract = AdvSimd.Multiply(p, Cwww);
        x_fract = AdvSimd.Subtract(AdvSimd.Multiply(Vector128.Create(2.0f), x_fract), Vector128<float>.One);

        var h  = AdvSimd.Subtract(AdvSimd.Abs(x_fract), Vector128.Create(0.5f));
        var ox = AdvSimd.Floor(AdvSimd.Add(x_fract, Vector128.Create(0.5f)));
        var a0 = AdvSimd.Subtract(x_fract, ox);

        m = AdvSimd.Multiply(
                             m,
                             AdvSimd.Subtract(
                                              Vector128.Create(1.79284291400159f),
                                              AdvSimd.Multiply(
                                                               Vector128.Create(0.85373472095314f),
                                                               AdvSimd.Add(
                                                                           AdvSimd.Multiply(a0, a0),
                                                                           AdvSimd.Multiply(h, h)
                                                                          )
                                                              )
                                             )
                            );

        var g0 = AdvSimd.FusedMultiplyAdd(a0, x0, h);
        var g1 = AdvSimd.FusedMultiplyAdd(a0, x12, h);

        var dot_m_g0 = AdvSimd.Multiply(m, g0);
        var dot_m_g1 = AdvSimd.Multiply(m, g1);

        var noise = AdvSimd.Add(AdvSimd.Multiply(Vector128.Create(130.0f), dot_m_g0), AdvSimd.Multiply(Vector128.Create(130.0f), dot_m_g1));

        return noise;
    }
}