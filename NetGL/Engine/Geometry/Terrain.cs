using OpenTK.Mathematics;

namespace NetGL;

public class Terrain: IShape<Terrain> {
    public readonly int width;
    public readonly int height;
    public readonly int offset_x;
    public readonly int offset_y;
    public readonly Plane plane;
    public readonly int points_per_unit = 10;

    public Terrain(Plane plane, int width, int height, int offset_x, int offset_y) {
        this.plane = plane;
        this.width = width;
        this.height = height;
        this.offset_x = offset_x;
        this.offset_y = offset_y;
    }

    public IShapeGenerator generate() => new TerrainShapeGenerator(this);
}

file class TerrainShapeGenerator: IShapeGenerator {
    private readonly Terrain terrain;

    public TerrainShapeGenerator(Terrain terrain) {
        this.terrain = terrain;
    }

    public IEnumerable<Vector3> get_vertices() {
        var n = new FastNoiseLite();
        n.SetFractalOctaves(12);
        n.SetFractalType(FastNoiseLite.FractalType.Ridged);
        n.SetFrequency(0.01f);
        n.SetFractalLacunarity(2f);
        n.SetFractalGain(0.21f);

        for (int x = 0; x < terrain.width * terrain.points_per_unit + 1; ++x) {
            for (int y = 0; y < terrain.height * terrain.points_per_unit + 1; ++y) {
                yield return terrain.plane[(float)x / terrain.points_per_unit, (float)y / terrain.points_per_unit, n.GetNoise((float)x / terrain.points_per_unit, (float)y / terrain.points_per_unit) * 7.5f];
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
        for (int x = 1; x < terrain.width * terrain.points_per_unit + 1; ++x) {
            for (int y = 1; y < terrain.height * terrain.points_per_unit + 1; ++y) {
                var bottom_left = (y - 1) + (x - 1) * (terrain.width * terrain.points_per_unit + 1);
                var bottom_right = (y - 1) + x * (terrain.width * terrain.points_per_unit+ 1);
                var top_left = y + (x - 1) * (terrain.width * terrain.points_per_unit + 1);
                var top_right = y + x * (terrain.width * terrain.points_per_unit + 1);

                yield return (bottom_left, bottom_right, top_left);
                yield return (top_left, bottom_right, top_right);
            }
        }
    }
}