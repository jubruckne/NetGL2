using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NetGL;

BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser(true)]
public class Benchmark {
    private NativeArray<float> hm1;
    private NativeArray<float> hm2;
    private Rectangle<int> area;
    private Rectangle<int> texture_size;

    public List<(float freq, float amp)> octaves { get; set; } = [
        (0.8f, 355f),
        (5.0f, 25f),
        (20.1f, 12f),
        (39.5f, 6.7f)
    ];

    [GlobalSetup]
    public void Setup() {
        area = Rectangle.centered_at((0, 0), 16384);
        texture_size = Rectangle.with_size(1024 * 4, 1024 * 4);

        hm1 = new NativeArray<float>(texture_size.width * texture_size.height);
        hm2 = new NativeArray<float>(texture_size.width * texture_size.height);

        SimplexNoise2.generate_2d<SimplexKernel2>(area, texture_size, hm1.get_view<float>(), octaves.ToArray(), 4);
        SimplexNoise2.generate_2d<SimplexKernel3>(area, texture_size, hm2.get_view<float>(), octaves.ToArray(), 4);

        for(int i = 0; i < hm1.length; ++i)
            if (Math.Abs(hm1[i] - hm2[i]) > 0.0005)
                throw new Exception($"{hm1[i]} != {hm2[i]}");
    }


    [Benchmark] public void Simplex2()
        => SimplexNoise2.generate_2d<SimplexKernel2>(area, texture_size, hm1.get_view<float>(), octaves.ToArray(), 1);

    [Benchmark] public void Simplex3()
        => SimplexNoise2.generate_2d<SimplexKernel3>(area, texture_size, hm1.get_view<float>(), octaves.ToArray(), 1);




}