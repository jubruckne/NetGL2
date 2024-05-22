using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace NetGL;

public readonly struct SimplexKernel: IKernel {
    private static readonly float F2 = 0.5f * (MathF.Sqrt(3.0f) - 1.0f);
    private static readonly float G2 = (3.0f - MathF.Sqrt(3.0f)) / 6.0f;
    private static readonly float G2_times_2_minus_1 = 2.0f * G2 - 1.0f;

    static Vector128<float> IKernel.evaluate(Vector128<float> xx, Vector128<float> yy) {
        var vec_s  = (xx + yy) * F2;
        var vec_xs = xx + vec_s;
        var vec_ys = yy + vec_s;
        var vec_i  = Vector128.Floor(vec_xs); //var vec_i = FastFloor(vec_xs);
        var vec_j  = Vector128.Floor(vec_ys); //var vec_j = FastFloor(vec_ys);

        var vec_t  = (vec_i + vec_j) * G2;
        var vec_X0 = vec_i - vec_t;
        var vec_Y0 = vec_j - vec_t;
        var vec_x0 = xx - vec_X0;
        var vec_y0 = yy - vec_Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        var mask   = AdvSimd.CompareGreaterThan(vec_x0, vec_y0);
        var vec_i1 = Vector128.ConditionalSelect(mask, Vector128<float>.One, Vector128<float>.Zero);
        var vec_j1 = Vector128.ConditionalSelect(mask, Vector128<float>.Zero, Vector128<float>.One);

        var vec_x1 = vec_x0 - vec_i1 + Vector128.Create(G2);
        var vec_y1 = vec_y0 - vec_j1 + Vector128.Create(G2);
        var vec_x2 = vec_x0 + Vector128.Create(G2_times_2_minus_1);
        var vec_y2 = vec_y0 + Vector128.Create(G2_times_2_minus_1);

        // Calculate squared distances for each corner
        var vec_t0 =
            Vector128.Create(0.5f) -
            AdvSimd.FusedMultiplyAdd(
                                     vec_x0 * vec_x0,
                                     vec_y0,
                                     vec_y0
                                    );

        var vec_t1 =
            Vector128.Create(0.5f) -
            AdvSimd.FusedMultiplyAdd(
                                     vec_x1 * vec_x1,
                                     vec_y1,
                                     vec_y1
                                    );

        var vec_t2 =
            Vector128.Create(0.5f) -
            AdvSimd.FusedMultiplyAdd(
                                     vec_x2 * vec_x2,
                                     vec_y2,
                                     vec_y2
                                    );

        var vz_0 = AdvSimd.CompareLessThan(vec_t0, Vector128<float>.Zero);
        var vz_1 = AdvSimd.CompareLessThan(vec_t1, Vector128<float>.Zero);
        var vz_2 = AdvSimd.CompareLessThan(vec_t2, Vector128<float>.Zero);

        vec_t0 = vec_t0 * vec_t0;
        vec_t0 = vec_t0 * vec_t0;

        vec_t1 = vec_t1 * vec_t1;
        vec_t1 = vec_t1 * vec_t1;

        vec_t2 = vec_t2 * vec_t2;
        vec_t2 = vec_t2 * vec_t2;

        vec_t0 = Vector128.ConditionalSelect(vz_0, Vector128<float>.Zero, vec_t0);
        vec_t1 = Vector128.ConditionalSelect(vz_1, Vector128<float>.Zero, vec_t1);
        vec_t2 = Vector128.ConditionalSelect(vz_2, Vector128<float>.Zero, vec_t2);

        var vec_n0 = vec_t0 * grad_2d(
                                      Vector128.ConvertToInt32(vec_i),
                                      Vector128.ConvertToInt32(vec_j),
                                      vec_x0,
                                      vec_y0
                                     );

        var vec_n1 = vec_t1 * grad_2d(
                                      Vector128.ConvertToInt32(vec_i + vec_i1),
                                      Vector128.ConvertToInt32(vec_j + vec_j1),
                                      vec_x1,
                                      vec_y1
                                     );
        var vec_n2 = vec_t2 * grad_2d(
                                      Vector128.ConvertToInt32(vec_i + Vector128<float>.One),
                                      Vector128.ConvertToInt32(vec_j + Vector128<float>.One),
                                      vec_x2,
                                      vec_y2
                                     );

        return 40.0f * (vec_n0 + vec_n1 + vec_n2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float grad_2d(int hash, float x, float y) {
        var h = hash & 7;      // Convert low 3 bits of hash code
        var u = h < 4 ? x : y; // into 8 simple gradient directions,
        var v = h < 4 ? y : x; // and compute the dot product with (x,y).
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> grad_2d(Vector128<int> i, Vector128<int> j, Vector128<float> x, Vector128<float> y) {
        var h = i * 374761393 + j * 668265263;
        h = (h ^ (h >> 13)) * 1274126177;
        h = h ^ (h >> 16);

        return Vector128.Create(
                                grad_2d(h.GetElement(0), x.GetElement(0), y.GetElement(0)),
                                grad_2d(h.GetElement(1), x.GetElement(1), y.GetElement(1)),
                                grad_2d(h.GetElement(2), x.GetElement(2), y.GetElement(2)),
                                grad_2d(h.GetElement(3), x.GetElement(3), y.GetElement(3))
                               );
    }
}