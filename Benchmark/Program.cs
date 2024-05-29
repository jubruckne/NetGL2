using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using NetGL;

var summary = BenchmarkRunner.Run<Benchmark>();//(new MultipleRuntimesConfig());

[MemoryDiagnoser(true)]
public class Benchmark {
    private NativeArray<float> hm1;
    private NativeArray<float> hm2;

    public List<(float freq, float amp)> octaves { get; set; } = [
        (0.8f, 355f),
        (5.0f, 25f),
        (20.1f, 12f),
        (39.5f, 6.7f)
    ];

    private Noise.Settings noise_settings = new(
                                                Rectangle.centered_at((0f, 0f), 16384f),
                                                8192,
                                                [
                                                    (1.8f, 355f),
                                                    (5.0f, 25f),
                                                    (20.1f, 12f),
                                                    (39.5f, 16.7f)
                                                ]
                                               );

    [GlobalSetup]
    public void Setup() {
        hm1 = new NativeArray<float>(noise_settings.texture_size * noise_settings.texture_size);
        hm2 = new NativeArray<float>(noise_settings.texture_size * noise_settings.texture_size);

        Noise.generate_2d<SimplexKernel_SIMD, float>(noise_settings, hm1.get_view<float>(), 4);

        /*for(int i = 0; i < hm1.length; ++i)
            if (Math.Abs(hm1[i] - hm2[i]) > 0.0005)
                throw new Exception($"{hm1[i]} != {hm2[i]}");
                */
    }

    [Benchmark] public void SimplexKernel_SIMD()
        => Noise.generate_2d<SimplexKernel_SIMD, float>(noise_settings, hm1.get_view<float>(), 1);


}

public class MultipleRuntimesConfig : ManualConfig {
    public MultipleRuntimesConfig() {
        AddJob(Job.Default.WithRuntime(CoreRuntime.Core80));

        // Add console logger to see progress and results
        AddLogger(ConsoleLogger.Default);

        // Add default columns and exporters
        AddColumnProvider(DefaultColumnProviders.Instance);
    }
}