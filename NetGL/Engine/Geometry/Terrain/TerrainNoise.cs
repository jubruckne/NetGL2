namespace NetGL;

public sealed class TerrainNoise: Noise {
    public TerrainNoise(): base(333) {
        add_simplex_layer(0.001f, 25f);
        add_simplex_layer(0.003f, 12f);
        add_simplex_layer(0.01f, 3.5f);
        add_cellular_layer(1.0f, 0.135f);
    }

    public override float sample(in float x, in float y) {
        return base.sample(x, y);
    }
}