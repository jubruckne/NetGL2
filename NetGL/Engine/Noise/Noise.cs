using System.Numerics;
using System.Runtime.CompilerServices;

namespace NetGL;

using Libraries;
using OpenTK.Mathematics;

[SkipLocalsInit]
public abstract class Layer {
    public readonly float amplitude;
    public readonly float frequency;

    protected readonly FastNoiseLite generator = new();

    protected Layer(int seed, FastNoiseLite.NoiseType type, float frequency, float amplitude) {
        generator.SetSeed(seed);
        generator.SetNoiseType(type);
        generator.SetFrequency(frequency);
        this.amplitude = amplitude;
        this.frequency = frequency;
    }

    public abstract float generate(float x, float y);

    public abstract float this[float x, float y] { get; }
}

[SkipLocalsInit]
public sealed class PerlinLayer: Layer {
    public PerlinLayer(int seed, float frequency, float amplitude) :
        base(seed - 355875, FastNoiseLite.NoiseType.Perlin, frequency, amplitude) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float generate(float x, float y)
        => generator.SinglePerlin(x * frequency, y * frequency) * amplitude;

    public override float this[float x, float y] => generator.SinglePerlin(x * frequency, y * frequency) * amplitude;
}

[SkipLocalsInit]
public sealed class SimplexLayer: Layer {
    public SimplexLayer(int seed, float frequency, float amplitude):
        base(seed - 438050, FastNoiseLite.NoiseType.OpenSimplex2, frequency, amplitude) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float generate(float x, float y)
        => generator.SingleSimplex(x * frequency, y * frequency) * amplitude;

    public override float this[float x, float y] => generator.SingleSimplex(x * frequency, y * frequency) * amplitude;
}

[SkipLocalsInit]
public sealed class ValueLayer: Layer {
    public ValueLayer(int seed, float frequency, float amplitude):
        base(seed + 730236, FastNoiseLite.NoiseType.Value, frequency, amplitude) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float generate(float x, float y)
        => generator.SingleValue(x * frequency, y * frequency) * amplitude;

    public override float this[float x, float y] => generator.SingleValue(x * frequency, y * frequency) * amplitude;

}

[SkipLocalsInit]
public sealed class CellularLayer: Layer {
    public CellularLayer(int seed, float frequency, float amplitude) :
        base(seed + 886344, FastNoiseLite.NoiseType.Cellular, frequency, amplitude) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float generate(float x, float y)
        => generator.SingleCellular(x * frequency, y * frequency) * amplitude;

    public override float this[float x, float y] => generator.SingleCellular(x * frequency, y * frequency) * amplitude;
}

[SkipLocalsInit]
public class Noise {
    private readonly List<Layer> layers;
    private readonly int seed;

    public Noise(int seed = 1701) {
        this.seed = seed;
        this.layers = [];
    }

    public virtual float sample(float x, float y) {
        float sample = 0;

        for (var l = 0; l < layers.Count; ++l)
            sample += layers[l].generate(x, y);

        return sample;
    }

    public float sample(Vector2 p) {
        float sample = 0;

        for(var l = 0; l < layers.Count; ++l)
            sample += layers[l].generate(p.X, p.Y);

        return sample;
    }

    public Field<T> sample<T>(int width, int height, float offset_x, float offset_y, float stride_x, float stride_y, float scale = 1f) where T: unmanaged, ISignedNumber<T> {
        // Console.WriteLine("------>> sampling noise: " + width * height);
        var samples = new Field<T>(width, height);
        int x, y;
        float nx, ny;

        foreach (var l in layers) {
            var frequency = l.frequency;
            var amplitude = l.amplitude;
            var gen = l.generate;

            for (var i = 0; i < width * height; i++) {
                x = i % width;
                y = i / height;

                nx = (offset_x + x * stride_x) * frequency;
                ny = (offset_y + y * stride_y) * frequency;

                samples.by_ref(x, y) += T.CreateSaturating(gen(nx, ny) * amplitude * scale);
            }
        }

        return samples;
    }

    public void add_perlin_layer(float frequency, float amplitude) {
        layers.Add(new PerlinLayer(seed + layers.Count, frequency, amplitude));
    }

    public SimplexLayer add_simplex_layer(float frequency, float amplitude) {
        var layer = new SimplexLayer(seed + layers.Count, frequency, amplitude);
        layers.Add(layer);
        return layer;
    }

    public void add_value_layer(float frequency, float amplitude) {
        layers.Add(new ValueLayer(seed + layers.Count, frequency, amplitude));
    }

    public CellularLayer add_cellular_layer(float frequency, float amplitude) {
        var layer = new CellularLayer(seed + layers.Count, frequency, amplitude);
        layers.Add(layer);
        return layer;
    }
}