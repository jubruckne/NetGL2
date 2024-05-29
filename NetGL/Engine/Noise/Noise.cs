using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace NetGL;

public static class Noise {
    public class Settings {
        public readonly int texture_size;
        public readonly Rectangle<float> area;
        public readonly Octave[] octaves;

        public Settings(Settings from) {
            texture_size = from.texture_size;
            area = from.area;
            octaves = from.octaves.ToArray();
        }

        public Settings(Rectangle<float> area, int texture_size, Octave[] octaves) {
            Debug.assert(texture_size.is_power_of_2());

            this.texture_size = texture_size;
            this.area         = area;
            this.octaves      = octaves;
        }
    }

    public class Octave {
        public float frequency;
        public float amplitude;

        public Octave(Octave from) {
            this.frequency = from.frequency;
            this.amplitude = from.amplitude;
        }

        public Octave(float frequency, float amplitude) {
            this.frequency = frequency;
            this.amplitude = amplitude;
        }

        public static implicit operator Octave((float frequency, float amplitude) octave)
            => new(octave.frequency, octave.amplitude);
    }

    private static readonly ParallelOptions parallel_options = new ParallelOptions { MaxDegreeOfParallelism = 6 };

    private static unsafe void generate_2d_internal<TKernel, TFloat>(Rectangle<float> area,
                                                             int texture_size,
                                                             TFloat* data,
                                                             Vector128<float> frequencies,
                                                             Vector128<float> amplitudes,
                                                             int threads
    )
        where TKernel: IKernel where TFloat: unmanaged, IFloatingPointIeee754<TFloat> {

        parallel_options.MaxDegreeOfParallelism = threads;

        Parallel.For(0,
                     texture_size / 128,
                     parallel_options,
                     body
                    );
        return;

        void body(int row)
            => generate_2d_internal<TKernel, TFloat>(
                                                     area,
                                                     texture_size,
                                                     row * 128,
                                                     row * 128 + 128,
                                                     data,
                                                     frequencies,
                                                     amplitudes
                                                    );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void generate_2d_internal<TKernel, TFloat>(Rectangle<float> area,
                                                             int texture_size,
                                                             int start_row,
                                                             int end_row,
                                                             TFloat* data,
                                                             Vector128<float> frequencies,
                                                             Vector128<float> amplitudes
    )
        where TKernel: IKernel where TFloat: unmanaged, IFloatingPointIeee754<TFloat> {

        var f_texture_size = (float)texture_size;

        for (var x = 0; x < texture_size; ++x) {
            var xx = Vector128.Create(area.x) + Vector128.Create(x / f_texture_size) * frequencies;
            for (var y = start_row; y < end_row; ++y) {
                var yy = Vector128.Create(area.y) + Vector128.Create(y / f_texture_size) * frequencies;
                data[x + y * texture_size] =
                    TFloat.CreateTruncating(Vector128.Sum(TKernel.evaluate(xx, yy) * amplitudes));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void generate_2d<TKernel, TFloat>(Settings settings,
                                                           ArrayView<TFloat> data,
                                                           int threads
    )
        where TKernel: IKernel
        where TFloat: unmanaged, IFloatingPointIeee754<TFloat> {
        if (threads == 0)
            Error.invalid_argument(threads);

        Debug.assert_equal(data.length, settings.texture_size * settings.texture_size);

        var amplitude_sum = 0f;
        foreach (var o in settings.octaves)
            amplitude_sum += o.amplitude;

        if (amplitude_sum == 0)
            Error.exception("amplitude is zero!");

        if (settings.octaves.Length == 4) {
            var amplitudes = Vector128.Create(
                                              settings.octaves[0].amplitude,
                                              settings.octaves[1].amplitude,
                                              settings.octaves[2].amplitude,
                                              settings.octaves[3].amplitude
                                             );
            var frequencies = Vector128.Create(
                                               settings.octaves[0].frequency,
                                               settings.octaves[1].frequency,
                                               settings.octaves[2].frequency,
                                               settings.octaves[3].frequency
                                              );

            generate_2d_internal<TKernel, TFloat>(
                                          settings.area,
                                          settings.texture_size,
                                          data.get_pointer(),
                                          frequencies,
                                          amplitudes,
                                          threads
                                         );
            return;
        }

        Error.invalid_argument(settings.octaves.Length, "need be 4");
    }
}