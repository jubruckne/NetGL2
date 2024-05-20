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
                     texture_size.width / 128,
                     parallel_options,
                     ii => {
                         for (var i = ii * 128; i < ii * 128 + 128; ++i) {
                             var xx = Vector128.Create(i / (float)texture_size.width) * frequencies;
                             for (var j = 0; j < texture_size.height; j++) {
                                 var yy = Vector128.Create(j / (float)texture_size.height) * frequencies;
                                 data[i + texture_size.width * j] =
                                     Vector128.Sum(
                                                   TKernel.evaluate(xx, yy)
                                                   * amplitudes
                                                  );
                             }
                         }
                     }
                    );
    }

    public static unsafe void generate_2d<TKernel>(Rectangle<int> area,
                                                   Rectangle<int> texture_size,
                                                   ArrayView<float> data,
                                                   (float frequency, float amplitude)[] octaves,
                                                   int threads) where TKernel: IKernel {
        if(threads == 0)
            Error.invalid_argument(threads);

        if (data.length != texture_size.get_area())
            Error.exception("data.Length != width * height");

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