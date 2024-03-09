using OpenTK.Mathematics;

namespace NetGL;

using Libraries;

internal abstract class Layer {
    public float amplitude;

    internal readonly FastNoiseLite generator = new();

    protected Layer(int seed, FastNoiseLite.NoiseType type, float frequency, float amplitude) {
        generator.SetSeed(seed);
        generator.SetNoiseType(type);
        generator.SetFrequency(frequency);
        this.amplitude = amplitude;
    }
}

file class PerlinLayer: Layer {
    public PerlinLayer(int seed, float frequency, float amplitude):
        base(seed - 355875, FastNoiseLite.NoiseType.Perlin, frequency, amplitude) {}
}

file class SimplexLayer: Layer {
    public SimplexLayer(int seed, float frequency, float amplitude):
        base(seed - 438050, FastNoiseLite.NoiseType.OpenSimplex2, frequency, amplitude) {}
}

file class ValueLayer: Layer {
    public ValueLayer(int seed, float frequency, float amplitude):
        base(seed + 730236, FastNoiseLite.NoiseType.Value, frequency, amplitude) {}
}

file class CellularLayer: Layer {
    public CellularLayer(int seed, float frequency, float amplitude) :
        base(seed + 886344, FastNoiseLite.NoiseType.Cellular, frequency, amplitude) {
    }
}

public class Noise {
    private readonly List<Layer> layers;
    private readonly int seed;

    public Noise(int seed = 1701) {
        this.seed = seed;
        this.layers = [];
    }

    public float sample(in Vector2 p) {
        float sample = 0;

        for(int l = 0; l < layers.Count - 1; l ++) {
            sample += layers[l].generator.GetNoise(p.X, p.Y) * layers[l].amplitude;
        }

        return sample;
    }

    public float[,] sample(int width, int height, float offset_x, float offset_y, float stride_x, float stride_y) {
        var samples = new float[width, height];

        int x, y;
        float nx, ny;

        foreach (var l in layers) {
            for (var i = 0; i < (width * height); i++) {
                x = i % width;
                y = i / height;

                nx = offset_x + x * stride_x;
                ny = offset_y + y * stride_y;

                samples[x, y] += l.generator.GetNoise(nx, ny) * l.amplitude;
            }
        }

        return samples;
    }

    public void add_perlin_layer(float frequency, float amplitude) {
        layers.Add(new PerlinLayer(seed + layers.Count, frequency, amplitude));
    }

    public void add_simplex_layer(float frequency, float amplitude) {
        layers.Add(new SimplexLayer(seed + layers.Count, frequency, amplitude));
    }

    public void add_value_layer(float frequency, float amplitude) {
        layers.Add(new ValueLayer(seed + layers.Count, frequency, amplitude));
    }

    public void add_cellular_layer(float frequency, float amplitude) {
        layers.Add(new CellularLayer(seed + layers.Count, frequency, amplitude));
    }
}