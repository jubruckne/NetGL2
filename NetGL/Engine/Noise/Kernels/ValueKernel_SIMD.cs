using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace NetGL;

public class ValueKernel_SIMD : IKernel {
    private static readonly int[] perm = new int[256];
    private static readonly Random random = new Random();

    static ValueKernel_SIMD() {
        for (var i = 0; i < 256; i++)
            perm[i] = i;

        // Shuffle the permutation array
        for (var i = 0; i < 256; i++) {
            var j = random.Next(256);
            (perm[i], perm[j]) = (perm[j], perm[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> permute(Vector128<int> x)
        => Vector128.Create(
                            perm[x.GetElement(0) & 255],
                            perm[x.GetElement(1) & 255],
                            perm[x.GetElement(2) & 255],
                            perm[x.GetElement(3) & 255]
                           );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> fade(Vector128<float> t)
        => t * t * t * (t * (t * Vector128.Create(6f) - Vector128.Create(15f)) + Vector128.Create(10f));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> lerp(Vector128<float> t, Vector128<float> a, Vector128<float> b)
        => a + t * (b - a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> grad(Vector128<int> hash, Vector128<float> x, Vector128<float> y) {
        Vector128<int>   h = hash & Vector128.Create(3);
        Vector128<float> u = Vector128.ConditionalSelect(Vector128.LessThan(h, Vector128.Create(2)).AsSingle(), x, y);
        Vector128<float> v = Vector128.ConditionalSelect(Vector128.LessThan(h, Vector128.Create(2)).AsSingle(), y, x);

        Vector128<float> mask1 = Vector128.Equals(h & Vector128.Create(1), Vector128<int>.Zero).AsSingle();
        Vector128<float> mask2 = Vector128.Equals(h & Vector128.Create(2), Vector128<int>.Zero).AsSingle();

        Vector128<float> resU = Vector128.ConditionalSelect(mask1, u, -u);
        Vector128<float> resV = Vector128.ConditionalSelect(mask2, v, -v);

        return resU + resV;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> evaluate(Vector128<float> x, Vector128<float> y) {
        Vector128<int> xi0 = Vector128.ConvertToInt32(x);
        Vector128<int> yi0 = Vector128.ConvertToInt32(y);

        Vector128<float> xf = x - Vector128.ConvertToSingle(xi0);
        Vector128<float> yf = y - Vector128.ConvertToSingle(yi0);

        Vector128<float> u = fade(xf);
        Vector128<float> v = fade(yf);

        Vector128<int> xi1 = xi0 + Vector128.Create(1);
        Vector128<int> yi1 = yi0 + Vector128.Create(1);

        Vector128<int> aa = permute(permute(xi0) + yi0);
        Vector128<int> ab = permute(permute(xi0) + yi1);
        Vector128<int> ba = permute(permute(xi1) + yi0);
        Vector128<int> bb = permute(permute(xi1) + yi1);

        Vector128<float> x1 = xf - Vector128.Create(1.0f);
        Vector128<float> y1 = yf - Vector128.Create(1.0f);

        Vector128<float> n0 = grad(aa, xf, yf);
        Vector128<float> n1 = grad(ba, x1, yf);
        Vector128<float> n2 = grad(ab, xf, y1);
        Vector128<float> n3 = grad(bb, x1, y1);

        Vector128<float> lerp1 = lerp(u, n0, n1);
        Vector128<float> lerp2 = lerp(u, n2, n3);

        Vector128<float> result = lerp(v, lerp1, lerp2);

        return result;
    }
}