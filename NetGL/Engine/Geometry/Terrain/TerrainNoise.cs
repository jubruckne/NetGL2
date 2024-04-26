namespace NetGL;

public sealed class TerrainNoise: Noise {
    public TerrainNoise(): base(333) {
        add_simplex_layer(0.00001f, 375f);
        add_simplex_layer(0.00025f, 19f);
        add_simplex_layer(0.0015f, 2.9f);
        add_cellular_layer(0.5f, 0.1f);
    }

    public override float sample(in float x, in float y) {
        return base.sample(x, y);
    }
}