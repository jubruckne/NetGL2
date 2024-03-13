namespace NetGL;

using ECS;
using OpenTK.Mathematics;

public class Terrain: Entity {
    public readonly Plane plane;
    public readonly Material material;
    public readonly VertexArrayRenderer renderer;

    public const int max_resolution = 96;
    public readonly int chunk_size = 96;

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

        var chunk = allocate_chunk_at_world_position(new Vector3(0, 0, 0), 12);
        this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.vertex_array!, material));
        renderer.wireframe = false;
        this.add_behavior(_ => update());
    }

    private TerrainChunk allocate_chunk_at_world_position(in Vector3 position, byte neighbors = 0) {
        var key = TerrainChunk.Key.from_world_position(this, position);
        var chunk = chunks.allocate(key);

        for (byte n = 0; n < neighbors; n++) {
            foreach (var neighbor_chunk in key.neighbors(n)) {
                if(!chunks.is_allocated(neighbor_chunk))
                    chunks.allocate(neighbor_chunk);
            }
        }

        return chunk;
    }

    private TerrainChunk allocate_chunk(TerrainChunk.Key key) {
        var current_key = TerrainChunk.Key.from_world_position(this, camera.transform.position);
        var distance = (int)Math.Sqrt(Math.Pow(key.x - current_key.x, 2) + Math.Pow(key.y - current_key.y, 2));

        Console.WriteLine($"Allocating chunk (key={key}, dist={distance})");

        var (priority, resolution) = distance switch {
            0 => (0, max_resolution),
            1 => (1, max_resolution / 2),
            2 => (2, max_resolution / 4),
            3 => (3, max_resolution / 16),
            4 => (4, max_resolution / 32),
            _ => (6 + distance, max_resolution / 32)
        };

        var chunk = new TerrainChunk(this, key);
        chunk.create(resolution, priority);
        return chunk;
    }

    private (Vector2 position, float height) get_terrain_position(in Vector3 world_position) {
        var (tp, _) = plane.world_to_point_on_plane(world_position);
        return (tp, noise.sample(tp));
    }

    private void update() {
        return;

        var terrain_height = get_terrain_position(camera.transform.position).height + 8f;
        if (camera.transform.position.Y < terrain_height) {
            camera.transform.position.Y = float.Lerp(camera.transform.position.Y, terrain_height, 0.09f);
        } else if (camera.transform.position.Y > terrain_height) {
            camera.transform.position.Y = float.Lerp(terrain_height, camera.transform.position.Y, 0.12f);
        }

        var chunk_key = TerrainChunk.Key.from_world_position(this, camera.transform.position);

        //Console.WriteLine($"Player position={camera.transform.position}, terrain_key={chunk_key}, terrain_pos={TerrainChunk.Key.to_world_position(this, chunk_key, terrain_height - 8f)}");
        if (chunks.is_allocated(chunk_key)) {
            if (chunks[chunk_key].resolution < max_resolution) {
                Console.WriteLine($"requesting new chunk: {chunk_key}, world_pos={camera.transform.position}");
                //chunks[chunk_key].update(max_resolution);
            }
        } else {
            Console.WriteLine($"requesting new chunk: {chunk_key}, world_pos={camera.transform.position}");
            //chunks.allocate(chunk_key);
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
        this.pixel_count = chunk.terrain.chunk_size * chunk.resolution / 16;
        Console.WriteLine($"res={chunk.resolution}, size={pixel_count} x {pixel_count}");
    }

    public IEnumerable<Vector3> get_vertices() {
        var pos = chunk.center;

        var n = chunk.terrain.noise.sample(
            pixel_count + 1, pixel_count + 1,
            pos.X, pos.Y,
            (float)chunk.terrain.chunk_size / pixel_count, (float)chunk.terrain.chunk_size / pixel_count
        );

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