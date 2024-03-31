namespace NetGL;

using ECS;
using OpenTK.Mathematics;

public class Terrain: Entity {
    public readonly Plane plane;
    public readonly Material material;
    public readonly VertexArrayRenderer renderer;

    private readonly Quadtree<TerrainChunk> chunks;
    private readonly Camera camera;

    internal readonly Noise noise;

    internal Terrain(Plane plane, Entity? parent = null): base("Terrain", parent) {
        this.plane = plane;

        camera = get<Camera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();
        noise.add_simplex_layer(0.001f, 35f);
        noise.add_simplex_layer(0.002f, 35f);
        noise.add_simplex_layer(0.01f, 2.5f);
        noise.add_value_layer(2.5f, 0.035f);

        this.material = this.add_material(Material.random).material;
        this.renderer = this.add_vertex_array_renderer();
        this.chunks = new(32f, 2, allocate_chunk);

        Console.WriteLine($"Terrain: {chunks}");
        using var heightmap = noise.sample<half>(256, 256, 0, 0, 0.1f, 0.1f, 10);


        Garbage g = new("heighmap average");
        Console.WriteLine(heightmap.average());
        g.Dispose();

        g = new("heighmap median");
        Console.WriteLine(heightmap.median());
        g.Dispose();

        g = new("heighmap minimum");
        Console.WriteLine(heightmap.minimum());
        g.Dispose();

        g = new("heighmap maximum");
        Console.WriteLine(heightmap.maximum());
        g.Dispose();

        //heightmap.save_to_file("heightmap.json");
        //Console.WriteLine(heightmap);
        Console.WriteLine();
        Console.WriteLine("Existing terrain chunks:");

        foreach (var q in chunks)
            Console.WriteLine(q);


        var chunk = chunks.request_node(0, 0, 0);
        chunks.request_node(20, 0, 0);
        chunks.request_node(20, -127.3f, 0);

        Console.WriteLine();
        Console.WriteLine("Existing terrain chunks:");
        foreach (var q in chunks)
            Console.WriteLine(q);

        this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.data.vertex_array, material));
        renderer.wireframe = true;
        this.add_behavior(_ => update());
    }

    private TerrainChunk allocate_chunk(in Bounds bounds, int level) {
        var distance = world_to_terrain_distance(bounds.center.x, bounds.center.y);

        var chunk = new TerrainChunk(this, bounds);
        chunk.create(priority: 0);
        return chunk;
    }

    private (float terrain_x, float terrain_y, float height) world_to_terrain_position(in Vector3 world_position) {
        var tp = plane.world_to_point_on_plane_2d(world_position);
        return (tp.X, tp.Y, noise.sample(tp));
    }

    public float world_to_terrain_distance(float terrain_x, float terrain_y) {
        var pos = world_to_terrain_position(camera.transform.position);
        return MathF.Sqrt(
                          (pos.terrain_x - terrain_x) * (pos.terrain_x - terrain_x)
                          + (pos.terrain_y - terrain_y) * (pos.terrain_y - terrain_y)
                         );
    }

    public float world_to_terrain_distance(in Vector3 world_position, float terrain_x, float terrain_y) {
        var pos = world_to_terrain_position(world_position);
        return MathF.Sqrt(
                          (pos.terrain_x - terrain_x) * (pos.terrain_x - terrain_x)
                          + (pos.terrain_y - terrain_y) * (pos.terrain_y - terrain_y)
                         );
    }

    private (Vector2 position, float height) world_to_terrain_position() {
        var tp = plane.world_to_point_on_plane_2d(camera.transform.position);
        return (tp, noise.sample(tp));
    }


    private void update() {
        var (terrain_pos, terrain_height) = world_to_terrain_position();
        return;
        if (camera.transform.position.Y < terrain_height) {
            camera.transform.position.Y = float.Lerp(camera.transform.position.Y, terrain_height, 0.09f);
        } else if (camera.transform.position.Y > terrain_height) {
            camera.transform.position.Y = float.Lerp(terrain_height, camera.transform.position.Y, 0.12f);
        }
/*
        //Console.WriteLine($"Player position={camera.transform.position}, terrain_key={chunk_key}, terrain_pos={TerrainChunk.Key.to_world_position(this, chunk_key, terrain_height - 8f)}");
        if (!chunks.is_allocated(terrain_pos.position.X, terrain_pos.position.Y, )) {
            if (chunks[chunk_key].resolution < max_resolution) {
                Console.WriteLine($"requesting new chunk: {chunk_key}, world_pos={camera.transform.position}");
                //chunks[chunk_key].update(max_resolution);
            }
        } else {
            Console.WriteLine($"requesting new chunk: {chunk_key}, world_pos={camera.transform.position}");
            //chunks.allocate(chunk_key);
        }
    }
*/
    }
}

public static class TerrainFab {
    public static Terrain create_terrain(this World world, in Plane plane) {
        var e = new Terrain(plane, world);
        world.add_entity(e);
        return e;
    }
}