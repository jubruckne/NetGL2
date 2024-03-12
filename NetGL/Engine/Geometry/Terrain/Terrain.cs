namespace NetGL;

using ECS;
using OpenTK.Mathematics;

public class Terrain : Entity {
    public readonly Plane plane;
    public readonly Material material;
    public readonly VertexArrayRenderer renderer;

    public const int max_resolution = 10;
    public readonly int chunk_size = 100;

    private readonly Grid<TerrainChunk, TerrainChunk.Key> chunks;
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

        var chunk = allocate_chunk_at_world_position(camera.transform.position);
        this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.vertex_array!, material));
        renderer.wireframe = false;
        this.add_behavior(_ => update());
    }

    private TerrainChunk allocate_chunk_at_world_position(in Vector3 position) {
        var chunk = TerrainChunk.Key.from_world_position(this, position);
        return allocate_chunk(chunk);
    }

    private TerrainChunk allocate_chunk(TerrainChunk.Key key) {
        var current_pos = TerrainChunk.Key.to_terrain_position(this, key);
        var chunk_pos = TerrainChunk.Key.to_terrain_position(this, key);
        var distance = Vector2.Distance(current_pos, chunk_pos);

        Console.WriteLine($"Allocating chunk (key={key}, pos={chunk_pos}, dist={distance})");

        var resolution = 1;
        var priority = 99;

        if(key.x == 0 && key.y == 0)
            (priority, resolution) = (0, max_resolution);
        else if (distance <= chunk_size)
            (priority, resolution) = (0, max_resolution);
        //else if (distance <= chunk_size * 2)
        //    resolution = max_resolution;
        //else if (distance <= chunk_size * 4)
        //    resolution = max_resolution / 2;

        var chunk = new TerrainChunk(this, key);
        chunk.create(resolution, priority);
        return chunk;
    }

    public float get_height_at_world_position(in Vector3 world_position) {
        var (tp, _) = plane.world_to_point_on_plane(world_position);
        return noise.sample(tp);
    }

    private void update() {
        return;
        var h = get_height_at_world_position(camera.transform.position) + 8f;
        if (camera.transform.position.Y < h) {
            camera.transform.position.Y = float.Lerp(camera.transform.position.Y, h, 0.09f);
        } else if (camera.transform.position.Y > h) {
            camera.transform.position.Y = float.Lerp(h, camera.transform.position.Y, 0.12f);
        }

        var chunk_id = TerrainChunk.Key.from_world_position(this, camera.transform.position);

        if (chunks.is_allocated(chunk_id)) {
            Console.WriteLine($"new chunk about to be allocated: {chunk_id}");
            if (chunks[chunk_id].resolution < max_resolution) {
                chunks[chunk_id].update(max_resolution);
            }
        } else {
            chunks.allocate(chunk_id);
        }
    }
}

public sealed class TerrainShapeGenerator : IShapeGenerator {
    private readonly TerrainChunk chunk;
    private readonly Plane plane;
    private readonly int pixel_count;

    internal TerrainShapeGenerator(TerrainChunk chunk) {
        this.chunk = chunk;
        this.plane = chunk.terrain.plane;
        this.pixel_count = chunk.terrain.chunk_size * chunk.resolution;
    }

    public IEnumerable<Vector3> get_vertices() {
        var pos = chunk.center;

        var n = chunk.terrain.noise.sample(
            pixel_count + 1, pixel_count + 1,
            pos.X, pos.Y,
            (float)chunk.terrain.chunk_size / pixel_count, (float)chunk.terrain.chunk_size / pixel_count
        );

        Console.WriteLine($"Chunk: xy = {pos.X},{pos.Y}");

        for (var x = 0; x < pixel_count + 1; x++) {
            for (var y = 0; y < pixel_count + 1; y++) {
                yield return plane.to_world(
                    pos.X + chunk.terrain.chunk_size * (float)x / pixel_count,
                    pos.Y + chunk.terrain.chunk_size * (float)y / pixel_count,
                    n[x, y]
                );
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
        for (var x = 1; x < pixel_count + 1; ++x) {
            for (var y = 1; y < pixel_count + 1; ++y) {
                var bottom_left = (y - 1) + (x - 1) * (pixel_count + 1);
                var bottom_right = (y - 1) + x * (pixel_count + 1);
                var top_left = y + (x - 1) * (pixel_count + 1);
                var top_right = y + x * (pixel_count + 1);

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