using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

using ECS;
using OpenTK.Mathematics;

/*
    Vertex
        Shared buffer (4 bytes)
            half x,
            half y
        Per-Chunk buffer (10 bytes)
            float height,
            half3 normal
    Instance buffer (6 bytes)
            int16 offset_x
            int16 offset_y
            int16 scale
            (width and height implicitly defined 96x96)
*/

public sealed class TerrainNoise: Noise {
    private readonly SimplexLayer biome;

    public TerrainNoise() {
        biome = add_simplex_layer(0.001f, 35f);
        //add_simplex_layer(0.002f, 35f);
        //add_simplex_layer(0.01f, 2.5f);
        add_value_layer(2.5f, 0.035f);
    }

    public override float sample(in float x, in float y) {
        var b =  biome.generate(x, y);

        if (b <= -0.25f)
            return -0.25f;

        return b * 50f;

        Console.WriteLine(b);
        return b;
    }
}

public class Terrain: Entity {
    public readonly VertexArrayRenderer renderer;

    //[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    //readonly Quadtree<TerrainChunk> tree;

    private readonly FirstPersonCamera camera;

    internal readonly TerrainNoise noise;
    private readonly Material[] material_per_level;
    private readonly Dictionary<Rectangle, TerrainChunk> chunks = new();
    private readonly LodLevels lod_levels;

    internal Terrain(Entity? parent = null): base("Terrain", parent) {
        //const int detail_levels = 9;
        //const int tile_size_at_highest_level = 32;

        camera = get<FirstPersonCamera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();

        // lod_levels = LodLevels.create(detail_levels, tile_size_at_highest_level);
        lod_levels = LodLevels.create([
            (16, 16),
            (32, 32),
            (64, 64),
            (128, 128),
            (256, 256),
            (512, 512),
            (1024, 4096)
        ]);

        // add separate material for each level
        material_per_level = new Material[lod_levels.count];
        for (var i = lod_levels.lowest.level; i <= lod_levels.highest.level; i++)
            material_per_level[i] = Material.random;

        renderer = this.add_vertex_array_renderer();

/*
        tree = new(64, max_level, allocate_chunk);
        var chunk = tree.request_node(0, 0, max_level);
        tree.request_node(-200, 0, max_level);
        tree.request_node(2100, 3200, 0);
*/
        chunks.Clear();

        Garbage.start_measuring(this);
        query_chunks_within_radius(0, 0, 2048);
        Garbage.stop_measuring(this);
        var chunk = chunks.Values.First();
        this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.vertex_array!, tesselate: false));
        //this.add_shader(Shader.from_file("terrain_shader", "terrain.vert.glsl", "terrain.frag.glsl"));

        /*
        foreach (var i in Enumerable.Range(1, 10)) {
            Console.WriteLine(
                              $"level_max_tile_size {chunks.level_max_tile_size} at distance {i * 100} = {camera.calculate_size_at_distance(chunks.level_max_tile_size, i * 100)}, resolution = {camera.calculate_resolution_at_distance(i * 100)}"
                             );
        }
*/
        //Console.WriteLine("\nQuadtree:");
        //TreePrinter.print(tree);
        //GC.Collect(6, GCCollectionMode.Default, true);



        renderer.render_settings.wireframe    = false;
        renderer.render_settings.depth_test   = true;
        renderer.render_settings.cull_face    = true;
        renderer.render_settings.front_facing = true;

        Console.WriteLine("Chunks:" + chunks.Count);


        this.add_behavior(_ => update());
    }

    private void query_chunks_within_radius(float x, float y, int radius) {
        var lod = lod_levels[0];

        //Console.WriteLine($"checking for chunks to render with maxsize = {lod.size}, {lod}");

        // Calculate bounds of the circle
        int2 p   = ((int)x.nearest_multiple(lod.tile_size), (int)y.nearest_multiple(lod.tile_size));

        split_chunk((x, y), radius, (p.x - lod.tile_size - radius, p.y - lod.tile_size - radius), (p.x + lod.tile_size + radius, p.y + lod.tile_size + radius), lod.level);
    }

    [SkipLocalsInit]
    private void split_chunk(in float2 position, int radius, in int2 bottom_left, in int2 top_right, short level) {
        var size      = lod_levels[level].tile_size;
        var half_size = lod_levels[level].half_tile_size;

        for (var x = bottom_left.x; x < top_right.x; x += size) {
            for (var y = bottom_left.y; y < top_right.y; y += size) {
                var center_x = x + half_size;
                var center_y = y + half_size;
                var distance = (int)MathF.Sqrt(
                                               MathF.Pow(center_x - position.x, 2)
                                               + MathF.Pow(center_y - position.y, 2)
                                              );
                var angle_to_chunk = MathF.Atan2(center_y - position.y, center_x - position.x);

                var isWithinFoV = MathF.Abs(0 - angle_to_chunk)
                                  <= camera.field_of_view_degrees.degree_to_radians() * 0.55f; // allow 10% margin

                if (distance < lod_levels[level].max_distance && level < lod_levels.max_level) {
                    // Debug.println(
                    //               $"Drill down to level {lod.level + 1} into Quad at ({center_x},{center_y}), distance={distance:N0}, lod={lod}.",
                    //               ConsoleColor.Magenta
                    //              );

                    split_chunk(position, radius, (x, y), (x + size, y + size), (short)(level + 1));
                } else if(distance <= radius) {
                    Debug.println(
                                  $"Quad at ({center_x},{center_y}), distance={distance:N0}, lod={level}.",
                                  ConsoleColor.Green
                                 );
                    var rect  = new Rectangle((x, y), size);
                    var chunk = new TerrainChunk(this, rect, material_per_level[level]);
                    chunks.Add(rect, chunk);
                    chunk.create(priority: 0);
                }
            }
        }
    }

    /*
    private TerrainChunk allocate_chunk(in Bounds bounds, int level) {
        var distance = world_to_terrain_distance(bounds.center.x, bounds.center.y);

        var chunk = new TerrainChunk(this, bounds, level, material_per_level[level]);
        chunk.create(priority: 0);
        return chunk;
    }*/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 terrain_to_world_position(float x, float y, float height)
        => new float3(x, height, -y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float terrain_x, float terrain_y, float height) world_to_terrain_position(in Vector3 world_position)
        => (world_position.X, -world_position.Z, noise.sample(world_position.X, -world_position.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (float terrain_x, float terrain_y, float height) world_to_terrain_position()
        => world_to_terrain_position(camera.transform.position);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    private void update() {
        var (terrain_x, terrin_y, terrain_height) = world_to_terrain_position();
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
    public static Terrain create_terrain(this World world) {
        var e = new Terrain(world);
        world.add_entity(e);
        return e;
    }
}