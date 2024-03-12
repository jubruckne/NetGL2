namespace NetGL;

using ECS;
using OpenTK.Mathematics;

public class Terrain : Entity {
    public readonly Plane plane;
    public readonly Material material;
    public readonly VertexArrayRenderer renderer;

    public const int max_resolution = 32;
    public readonly int chunk_size = 100;

    private readonly Grid<TerrainChunk> chunks;
    private readonly Camera camera;

    internal readonly Noise noise;

    internal Terrain(Plane plane, Entity? parent = null) : base("Terrain", parent) {
        this.plane = plane;

        camera = get<Camera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();
        noise.add_simplex_layer(0.001f, 35f);
        noise.add_simplex_layer(0.002f, 35f);
        noise.add_simplex_layer(0.01f, 2.5f);
        noise.add_value_layer(2.5f, 0.035f);

        chunks = new(allocate_chunk);

        material = this.add_material(Material.random).material;
        renderer = this.add_vertex_array_renderer();
        chunks.allocate(0, 0);
        this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunks[0, 0].vertex_array!, material));
        renderer.wireframe = true;
        this.add_behavior(_ => update());
    }

    private TerrainChunk allocate_chunk(short x, short y) {
        var (terrain_position, _) = plane.world_to_point_on_plane(camera.transform.position);
        var chunk_position = new Vector2(x * chunk_size, y * chunk_size);
        var distance = Vector2.Distance(terrain_position, chunk_position);

        var resolution = 1;
        var priority = 99;

        if(x == 0 && y == 0)
            (priority, resolution) = (0, max_resolution);
        else if (distance <= chunk_size)
            (priority, resolution) = (0, max_resolution);
        //else if (distance <= chunk_size * 2)
        //    resolution = max_resolution;
        //else if (distance <= chunk_size * 4)
        //    resolution = max_resolution / 2;

        var chunk = new TerrainChunk(this, x * chunk_size, y * chunk_size);
        chunk.create(resolution, priority);
        return chunk;
    }

    public float get_height_at_world_position(Vector3 world_position) {
        var (tp, _) = plane.world_to_point_on_plane(world_position);
        return noise.sample(tp);
    }

    private void update() {
        var (position, height) = plane.world_to_point_on_plane(camera.transform.position);

        var h = get_height_at_world_position(camera.transform.position) + 8f;
        if (camera.transform.position.Y < h) {
            camera.transform.position.Y = float.Lerp(camera.transform.position.Y, h, 0.09f);
        } else if (camera.transform.position.Y > h) {
            camera.transform.position.Y = float.Lerp(h, camera.transform.position.Y, 0.12f);
        }

        var x = (short)Math.Round(position.X / chunk_size);
        var y = (short)Math.Round(position.Y / chunk_size);

        if (chunks.is_allocated(x, y)) {
            if (chunks[x, y].resolution < max_resolution) {
                chunks[x, y].update(max_resolution);
            }
        } else {
            chunks.allocate(x, y);
        }
    }
}

public sealed class TerrainShapeGenerator : IShapeGenerator {
    private readonly TerrainChunk chunk;

    private readonly Plane plane;

    private readonly int size;
    private readonly int pixel_count;

    private readonly float offset_x;
    private readonly float offset_y;

    internal TerrainShapeGenerator(TerrainChunk chunk) {
        this.chunk = chunk;

        plane = chunk.terrain.plane;
        size = chunk.terrain.chunk_size;
        pixel_count = chunk.terrain.chunk_size * chunk.resolution;

        offset_x = chunk.x - size * 0.5f;
        offset_y = chunk.y - size * 0.5f;
    }

    public IEnumerable<Vector3> get_vertices() {
        var n = chunk.terrain.noise.sample(
            pixel_count + 1, pixel_count + 1,
            offset_x, offset_y,
            (float)size / chunk.resolution, (float)size / chunk.resolution
        );

        for (int x = 0; x < chunk.resolution + 1; x++) {
            for (int y = 0; y < chunk.resolution + 1; y++) {
                yield return plane.to_world(
                    (float)x * size / chunk.resolution + offset_x,
                    (float)y * size / chunk.resolution + offset_y, n[x, y]
                );
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
        for (int x = 1; x < chunk.resolution + 1; ++x) {
            for (int y = 1; y < chunk.resolution + 1; ++y) {
                var bottom_left = (y - 1) + (x - 1) * (chunk.resolution + 1);
                var bottom_right = (y - 1) + x * (chunk.resolution + 1);
                var top_left = y + (x - 1) * (chunk.resolution + 1);
                var top_right = y + x * (chunk.resolution + 1);

                yield return (bottom_left, bottom_right, top_left);
                yield return (top_left, bottom_right, top_right);
            }
        }
    }
}

public static class TerrainFab {
    public static Terrain create_terrain(this World world, in Plane plane) {
        var e = new Terrain(plane, world);
        world.add_entity(e);
        return e;
    }
}