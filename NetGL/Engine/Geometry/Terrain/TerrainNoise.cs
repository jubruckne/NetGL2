using System.Runtime.CompilerServices;
using NetGL.Libraries;

namespace NetGL;

[SkipLocalsInit]
public sealed class TerrainNoise: Noise {
    private readonly SimplexLayer s1;
    private readonly SimplexLayer s2;
    private readonly SimplexLayer s3;
    private readonly CellularLayer c1;
    internal readonly FastNoise2 fn2 = new FastNoise2("Simplex");

    public TerrainNoise(): base(333) {
        s1 = add_simplex_layer(0.0003f, 375f);
        s2 = add_simplex_layer(0.0015f, 49f);
        s3 = add_simplex_layer(0.015f, 2.9f);
        c1 = add_cellular_layer(1.29f, 0.5f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float sample(float x, float y) {
        return s1.generate(x, y) + s2.generate(y, x) + s3.generate(y, x) + c1.generate(x, y);;
    }
}