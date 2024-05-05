using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NetGL;

BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser(true)]
public class Benchmark {
    private readonly NativeArray<float> data1 = new NativeArray<float>(32 * 32 * 32, true);
    private readonly byte[] padd = new byte[1_182_011];

    private readonly NativeArray<float> data2 = new NativeArray<float>(32 * 32 * 32, true);

    [GlobalSetup]
    public void Setup() {
        var rnd = new Random(217924751);

        for (var i = 0; i < data1.length; ++i) {
            data1[i] = rnd.NextSingle();
            data2[i] = data1[i];
            padd[i % 17] = (byte)(i % 8);
        }
    }


    [Benchmark]
    public void SimpleLoop() {
        for (var i = 0; i < data1.length; ++i) {
            data1[i] = MathF.Cos(data1[i]);
        }
    }

    [Benchmark]
    public void Vectorized() {
        GaborNoise.Cos_aligned(data2.as_span());

        for (int i = 0; i < data1.length / 2048; ++i) {
            Console.WriteLine($"{data1[i]:N5}, {data2[i]:N5} : {data1[i] - data2[i]:N5}");
        }
    }
}