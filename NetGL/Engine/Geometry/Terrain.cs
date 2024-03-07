using OpenTK.Mathematics;

namespace NetGL;

public class Terrain: IShape<Terrain> {
    public readonly int width;
    public readonly int height;
    public readonly int offset_x;
    public readonly int offset_y;
    public readonly Plane plane;
    public readonly int resolution;

    public Terrain(Plane plane, int width, int height, int resolution, int offset_x, int offset_y) {
        this.plane = plane;
        this.width = width;
        this.height = height;
        this.offset_x = offset_x;
        this.offset_y = offset_y;
        this.resolution = resolution;
    }

    public IShapeGenerator generate() => new TerrainShapeGenerator(this);
}

file class TerrainShapeGenerator: IShapeGenerator {
    private readonly Terrain terrain;

    public TerrainShapeGenerator(Terrain terrain) {
        this.terrain = terrain;
    }

    public IEnumerable<Vector3> get_vertices() {
        var n1 = new FastNoiseLite(seed: 2345);
        n1.SetFractalOctaves(12);
        n1.SetFractalType(FastNoiseLite.FractalType.FBm);
        n1.SetFrequency(0.025f);
        n1.SetFractalLacunarity(2f);
        n1.SetFractalGain(0.29f);

        for (int x = 0; x < terrain.width * terrain.resolution + 1; ++x) {
            for (int y = 0; y < terrain.height * terrain.resolution + 1; ++y) {
                var height =
                    (n1.GetNoise((float)x / terrain.resolution - 18f, (float)y / terrain.resolution)) * 6.5f;

                yield return terrain.plane[(float)x / terrain.resolution, (float)y / terrain.resolution, height];
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
        for (int x = 1; x < terrain.width * terrain.resolution + 1; ++x) {
            for (int y = 1; y < terrain.height * terrain.resolution + 1; ++y) {
                var bottom_left = (y - 1) + (x - 1) * (terrain.width * terrain.resolution + 1);
                var bottom_right = (y - 1) + x * (terrain.width * terrain.resolution+ 1);
                var top_left = y + (x - 1) * (terrain.width * terrain.resolution + 1);
                var top_right = y + x * (terrain.width * terrain.resolution + 1);

                yield return (bottom_left, bottom_right, top_left);
                yield return (top_left, bottom_right, top_right);
            }
        }
    }
}