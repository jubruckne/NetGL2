using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace NetGL;

[SkipLocalsInit]
public static class Noise2D {
    private static int _seed;

    public static void calc_2d_bunched(Span<float> data, ReadOnlySpan<(float frequency, float amplitude)> octaves, int width, int height) {
        if(data.Length != width * height)
            Error.exception("data.Length != width * height");

        var amplitude_sum = 0f;
        foreach (var (_, amplitude) in octaves)
            amplitude_sum += amplitude;

        if(amplitude_sum == 0)
            Error.exception("amplitude is zero!");

        Vector128<float> xx, yy;

        if (octaves.Length == 4) {
            var amplitudes = Vector128.Create(
                                              octaves[0].amplitude,
                                              octaves[1].amplitude,
                                              octaves[2].amplitude,
                                              octaves[3].amplitude
                                             );
            var frequencies = Vector128.Create(
                                               octaves[0].frequency,
                                               octaves[1].frequency,
                                               octaves[2].frequency,
                                               octaves[3].frequency
                                              );

            for (var i = 0; i < width; i++) {
                xx = Vector128.Create(i / (float)width) * frequencies;
                for (var j = 0; j < height; j++) {
                    yy = Vector128.Create(j / (float)height) * frequencies;
                    data[i + width * j] = Vector128.Sum(generate_bunch(xx, yy) * amplitudes);
                }
            }
            return;
        }

        Error.invalid_argument(octaves.Length, "must be 4!");
    }

    private static IEnumerable<Range> batch(this int length, int batches) {
        int start = 0;
        int batch_size = length / batches;
        while (start < length) {
            yield return new(start, (start + batch_size).at_most(length));
            start += batch_size;
        }
    }

    public static void calc_2d(Rectangle<int> area, Rectangle<int> texture_size, Span<float> data, ReadOnlySpan<(float frequency, float amplitude)> octaves) {
        if(data.Length != texture_size.get_area())
            Error.exception("data.Length != width * height");

        var amplitude_sum = 0f;
        foreach (var (_, amplitude) in octaves)
            amplitude_sum += amplitude;

        if(amplitude_sum == 0)
            Error.exception("amplitude is zero!");

        float x, y;

        if (octaves.Length == 4) {
            Vector128<float> xx, yy;

            var amplitudes = Vector128.Create(
                                              octaves[0].amplitude,
                                              octaves[1].amplitude,
                                              octaves[2].amplitude,
                                              octaves[3].amplitude
                                             );
            var frequencies = Vector128.Create(
                                               octaves[0].frequency,
                                               octaves[1].frequency,
                                               octaves[2].frequency,
                                               octaves[3].frequency
                                              );

            for (var i = 0; i < texture_size.width; i++) {
                xx = Vector128.Create(i / (float)texture_size.width) * frequencies;
                for (var j = 0; j < texture_size.height; j++) {
                    yy = Vector128.Create(j / (float)texture_size.height) * frequencies;
                    data[i + texture_size.width * j] = Vector128.Sum(
                                                                     Vector128.Create(
                                                                          generate_2d(yy[0], xx[0]),
                                                                          generate_2d(xx[1], yy[1]),
                                                                          generate_2d(xx[2], yy[2]),
                                                                          generate_2d(xx[3], yy[3])
                                                                         )
                                                                     * amplitudes
                                                                    );
                }
            }
            return;
        }

        var noise_sum = 0f;
        for (var i = 0; i < texture_size.width; i++) {
            x = i / (float)texture_size.width;

            for (var j = 0; j < texture_size.height; j++) {
                y = j / (float)texture_size.height;

                noise_sum = 0f;

                foreach (var (freq, amplitude) in octaves)
                    noise_sum += generate_2d(x * freq, y * freq) * amplitude;

                data[i + texture_size.width * j] = noise_sum;
            }
        }
    }

    public static void Calc2D(Span<float> data, int width, int height, float frequency) {
        if(data.Length != width * height)
            Error.exception("data.Length != width * height");

        for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                data[i + width * j] = generate_2d(i * frequency, j * frequency);
    }

    private const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
    private const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0
    private const float G2_times_2 = 0.211324865f * 2f; // G2 = (3.0-Math.sqrt(3.0))/6.0

    private static readonly Vector128<float> v_f1 = Vector128.Create(1f);
    private static readonly Vector128<float> v_f2 = Vector128.Create(2f);
    private static readonly Vector128<float> v_f0 = Vector128.Create(0f);
    private static readonly Vector128<float> v_fG2 = Vector128.Create(G2);
    private static readonly Vector128<int> v_i255 = Vector128.Create(256 - 1);
    private static readonly Vector128<int> v_i7 = Vector128.Create(7);

    private static Vector128<float> generate_bunch(Vector128<float> x, Vector128<float> y) {
        // Skew the input space to determine which simplex cell we're in
        var s  = (x + y) * F2; // Hairy factor for 2D
        var xs = x + s;
        var ys = y + s;

        var i  = Vector128.Floor(xs);
        var j  = Vector128.Floor(ys);

        var t  = (i + j) * G2;
        var X0 = i - t; // Unskew the cell origin back to (x,y) space
        var Y0 = j - t;
        var x0 = x - X0; // The x,y distances from the cell origin
        var y0 = y - Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.

        var mask = AdvSimd.CompareGreaterThan(x0, y0);
        // Applying the mask using vbsl (Vector Bitwise Select)
        var i1 = AdvSimd.BitwiseSelect(mask, v_f1, v_f0);
        var j1 = AdvSimd.BitwiseSelect(mask, v_f0, v_f1);

        var x1 = x0 - i1 + v_fG2; // Offsets for middle corner in (x,y) unskewed coords
        var y1 = y0 - j1 + v_fG2;
        var x2 = x0 - v_f1 + v_f2 * v_fG2; // Offsets for last corner in (x,y) unskewed coords
        var y2 = y0 - v_f0 + v_f2 * v_fG2;

        // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
        var ii = mod_256(i);
        var jj = mod_256(j);

        // Convert integer vectors to float
        Vector128<float> ii_float = Vector128.ConvertToSingle(ii);
        Vector128<float> jj_float = Vector128.ConvertToSingle(jj);

        // Add floating-point offsets
        Vector128<float> ii1 = AdvSimd.Add(ii_float, i1);
        Vector128<float> jj1 = AdvSimd.Add(jj_float, j1);

        // Now ii1 and jj1 are Vector128<float> and can be used in further calculations
        // However, to use them as indices for gathering, you need to convert them back to integers
        Vector128<int> ii_indices = Vector128.ConvertToInt32(ii1);
        Vector128<int> jj_indices = Vector128.ConvertToInt32(jj1);

        // Use gathered indices to get perm values and calculate gradients
        Vector128<int> perm_ii = gather(ii_indices);
        Vector128<int> perm_jj = gather(jj_indices);

        // Compute combined indices for gradient calculations
        Vector128<int> combined_indices0 = AdvSimd.Add(perm_ii, perm_jj);
        Vector128<int> combined_indices1 = AdvSimd.Add(perm_ii, AdvSimd.Add(perm_jj, Vector128.ConvertToInt32(j1)));
        Vector128<int> combined_indices2 = AdvSimd.Add(perm_ii, AdvSimd.Add(perm_jj, Vector128.ConvertToInt32(v_f1)));

        // Calculate squared distances for each corner
        Vector128<float> t0 = AdvSimd.Subtract(Vector128.Create(0.5f), AdvSimd.Add(AdvSimd.Multiply(x0, x0), AdvSimd.Multiply(y0, y0)));
        Vector128<float> t1 = AdvSimd.Subtract(Vector128.Create(0.5f), AdvSimd.Add(AdvSimd.Multiply(x1, x1), AdvSimd.Multiply(y1, y1)));
        Vector128<float> t2 = AdvSimd.Subtract(Vector128.Create(0.5f), AdvSimd.Add(AdvSimd.Multiply(x2, x2), AdvSimd.Multiply(y2, y2)));

        // Conditionally compute contributions if t > 0, otherwise set to zero
        t0 = AdvSimd.BitwiseSelect(AdvSimd.CompareGreaterThan(t0, Vector128<float>.Zero), t0, Vector128<float>.Zero);
        t1 = AdvSimd.BitwiseSelect(AdvSimd.CompareGreaterThan(t1, Vector128<float>.Zero), t1, Vector128<float>.Zero);
        t2 = AdvSimd.BitwiseSelect(AdvSimd.CompareGreaterThan(t2, Vector128<float>.Zero), t2, Vector128<float>.Zero);

        // Compute gradients
        Vector128<float> n0 = AdvSimd.Multiply(AdvSimd.Multiply(t0, t0), grad(combined_indices0, x0, y0));
        Vector128<float> n1 = AdvSimd.Multiply(AdvSimd.Multiply(t1, t1), grad(combined_indices1, x1, y1));
        Vector128<float> n2 = AdvSimd.Multiply(AdvSimd.Multiply(t2, t2), grad(combined_indices2, x2, y2));

        // Combine noise contributions
        Vector128<float> totalNoise = AdvSimd.Add(AdvSimd.Add(n0, n1), n2);

        // Scale the result to the interval [-1,1]
        return AdvSimd.Multiply(totalNoise, Vector128.Create(40.0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> grad(Vector128<int> hash, Vector128<float> x, Vector128<float> y) {
        // Mask to get the lower 3 bits from each element in the hash vector
        Vector128<int> h = AdvSimd.And(hash, v_i7);
        Vector128<float> hFloat = Vector128.ConvertToSingle(h);

        // Determine the gradient vectors based on h
        Vector128<float> maskH = AdvSimd.CompareLessThan(hFloat, Vector128.Create(4.0f));

        // Select x or y based on h < 4
        Vector128<float> u = AdvSimd.BitwiseSelect(maskH, x, y);
        Vector128<float> v = AdvSimd.BitwiseSelect(maskH, y, x);

        // Convert 'h' bitwise checks to float to use in comparisons
        Vector128<float> hAnd1 = Vector128.ConvertToSingle(AdvSimd.And(h, Vector128.Create(1)));
        Vector128<float> hAnd2 = Vector128.ConvertToSingle(AdvSimd.And(h, Vector128.Create(2)));

        // Compute signs for u and v
        Vector128<float> maskU   = AdvSimd.CompareEqual(hAnd1, v_f0);
        Vector128<float> signedU = AdvSimd.BitwiseSelect(maskU, u, AdvSimd.Negate(u));

        Vector128<float> maskV   = AdvSimd.CompareEqual(hAnd2, v_f0);
        Vector128<float> scaledV = AdvSimd.Multiply(v, v_f2);
        Vector128<float> signedV = AdvSimd.BitwiseSelect(maskV, scaledV, AdvSimd.Negate(scaledV));

        // Add the results together to get the dot product
        return AdvSimd.Add(signedU, signedV);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> gather(Vector128<int> indices)
        => Vector128.Create(
                            _perm[indices.GetElement(0)],
                            _perm[indices.GetElement(1)],
                            _perm[indices.GetElement(2)],
                            _perm[indices.GetElement(3)]
                           );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> mod_256(Vector128<float> value) {
        // Apply modulus 256, since 256 is a power of two, use AND with 255
        return AdvSimd.And(Vector128.ConvertToInt32(value), v_i255);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float generate_2d(float x, float y) {
        float n0, n1, n2; // Noise contributions from the three corners

        // Skew the input space to determine which simplex cell we're in
        var s  = (x + y) * F2; // Hairy factor for 2D
        var xs = x + s;
        var ys = y + s;
        var i  = fast_floor(xs);
        var j  = fast_floor(ys);

        var t  = (i + j) * G2;
        var X0 = i - t; // Unskew the cell origin back to (x,y) space
        var Y0 = j - t;
        var x0 = x - X0; // The x,y distances from the cell origin
        var y0 = y - Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
        if (x0 > y0) {
            i1 = 1;
            j1 = 0;
        } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
        else {
            i1 = 0;
            j1 = 1;
        } // upper triangle, YX order: (0,0)->(0,1)->(1,1)

        // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
        // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
        // c = (3-sqrt(3))/6

        var x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1.0f + G2_times_2; // Offsets for last corner in (x,y) unskewed coords
        var y2 = y0 - 1.0f + G2_times_2;

        // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
        var ii = i & 255;  // Assume m = 256, so we use 255 as the bitmask. was: Mod(i, 256);
        var jj = j & 255;  // was: Mod(j, 256);

        // Calculate the contribution from the three corners
        var t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0.0f)
            n0 = 0.0f;
        else {
            t0 = t0 * t0;
            t0 = t0 * t0;
            n0 = t0 * grad_2d(_perm[ii + _perm[jj]], x0, y0);
        }

        var t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0.0f)
            n1 = 0.0f;
        else {
            t1 = t1 * t1;
            t1 = t1 * t1;
            n1 = t1 * grad_2d(_perm[ii + i1 + _perm[jj + j1]], x1, y1);
        }

        var t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0.0f)
            n2 = 0.0f;
        else {
            t2 = t2 * t2;
            t2 = t2 * t2;
            n2 = t2 * grad_2d(_perm[ii + 1 + _perm[jj + 1]], x2, y2);
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [-1,1].
        return 40.0f * (n0 + n1 + n2);
    }

    private static byte[] _perm;

    private static readonly byte[] PermOriginal = [
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69,
        142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
        117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74,
        165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105,
        92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187,
        208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227,
        47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
        101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
        112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
        145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
        50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
        78, 66, 215, 61, 156, 180, 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197,
        62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171,
        168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211,
        133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73,
        209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198,
        173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207,
        206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154,
        163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224,
        232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162,
        241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204,
        176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141,
        128, 195, 78, 66, 215, 61, 156, 180
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int fast_floor(float x)
        => x > 0 ? (int)x : (int)x - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float grad_2d(int hash, float x, float y) {
        var h = hash & 7;      // Convert low 3 bits of hash code
        var u = h < 4 ? x : y; // into 8 simple gradient directions,
        var v = h < 4 ? y : x; // and compute the dot product with (x,y).
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int perm(int x) {
        x = (x << 13) ^ x;
        return ((x * (x * x * 15731 + 789221) + 1376312589) & 0x7fffffff) % 256;
    }

    public static float calc_2d(int x, int y, float scale)
        => generate_2d(x * scale, y * scale);

    static Noise2D() {
        _perm = new byte[PermOriginal.Length];
        PermOriginal.CopyTo(_perm, 0);
    }

    public static int Seed {
        get => _seed;
        set {
            if (value == 0) {
                _perm = new byte[PermOriginal.Length];
                PermOriginal.CopyTo(_perm, 0);
            } else {
                _perm = new byte[512];
                var random = new Random(value);
                random.NextBytes(_perm);
            }

            _seed = value;
        }
    }
}