using System.Runtime.CompilerServices;

namespace NetGL;

using System.Runtime.Intrinsics;

[SkipLocalsInit]
public static class SimplexNoise2 {
    private static readonly ParallelOptions parallel_options = new ParallelOptions { MaxDegreeOfParallelism = 6 };

    private static unsafe void generate_2d_internal<TKernel>(Rectangle<int> area,
                                                             Rectangle<int> texture_size,
                                                             float* data,
                                                             Vector128<float> frequencies,
                                                             Vector128<float> amplitudes,
                                                             int threads
    )
        where TKernel: IKernel {

        parallel_options.MaxDegreeOfParallelism = threads;

        Parallel.For(0,
                     texture_size.height / 128,
                     parallel_options,
                     row => generate_2d_internal<TKernel>(
                                                          texture_size.width,
                                                          texture_size.height,
                                                          row * 128,
                                                          row * 128 + 128,
                                                          data,
                                                          frequencies,
                                                          amplitudes
                                                         )
                    );
    }

    private static unsafe void generate_2d_internal<TKernel>(int width,
                                                             int height,
                                                             int start_row,
                                                             int end_row,
                                                             float* data,
                                                             Vector128<float> frequencies,
                                                             Vector128<float> amplitudes
    )
        where TKernel: IKernel {

        var f_width  = (float)width;
        var f_height = (float)height;

        for (var x = 0; x < width; ++x) {
            var xx = Vector128.Create(x / f_width) * frequencies;
            for (var y = start_row; y < end_row; ++y) {
                var yy = Vector128.Create(y / f_height) * frequencies;
                data[x + y * width] =
                    Vector128.Sum(TKernel.evaluate(xx, yy) * amplitudes);
            }
        }
    }

    public static unsafe void generate_2d<TKernel>(Rectangle<int> area,
                                                   Rectangle<int> texture_size,
                                                   ArrayView<float> data,
                                                   (float frequency, float amplitude)[] octaves,
                                                   int threads) where TKernel: IKernel {
        if(threads == 0)
            Error.invalid_argument(threads);

        Debug.assert_equal(data.length, texture_size.get_area());

        var amplitude_sum = 0f;
        foreach (var (_, amplitude) in octaves)
            amplitude_sum += amplitude;

        if (amplitude_sum == 0)
            Error.exception("amplitude is zero!");

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

            generate_2d_internal<TKernel>(area, texture_size, data.get_pointer(), frequencies, amplitudes, threads);
            return;
        }

        Error.invalid_argument(octaves.Length, "need be 4");
    }
}