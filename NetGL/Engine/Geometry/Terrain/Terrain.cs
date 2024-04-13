using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData {
    public float2 offset;     // 8 x,z = position of the chunk
    public short size;        // 2 width and height of the chunk
    public short map_idx;     // 2 index into the heightmap (z)
}

// 3*4 = 12 bytes + 3 * 2 = 6 bytes = 18 bytes
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexData {
    public half height;      //  2 bytes
    public half3 normal;     //  6 bytes
}                            // 8 bytes

public class Terrain: Entity {
    public readonly VertexArrayRenderer renderer;

    //[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    //readonly Quadtree<TerrainChunk> tree;

    private readonly FirstPersonCamera camera;

    internal readonly TerrainNoise noise;
    internal readonly Texture2D<float> heightmaps;

    private readonly Material[] material_per_level;
    private readonly Dictionary<Rectangle, TerrainChunk> chunks = new();
    private readonly LodLevels lod_levels;
    private readonly VertexBuffer<InstanceData> instance_buffer;

    internal Terrain(Entity? parent = null): base("Terrain", parent) {
        const int detail_levels = 1;
        const int tile_size_at_highest_level = 512;

        camera = get<FirstPersonCamera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();

        lod_levels = LodLevels.create(detail_levels, tile_size_at_highest_level);

        heightmaps = new Texture2D<float>(
                                              4096,
                                              4096,
                                              PixelFormat.Red,
                                              PixelType.Float
                                             );
        heightmaps.wrap_t = TextureWrapMode.ClampToEdge;
        heightmaps.wrap_s = TextureWrapMode.ClampToEdge;
        heightmaps.min_filter = TextureMinFilter.Linear;
        heightmaps.mag_filter = TextureMagFilter.Linear;
        heightmaps.internal_pixel_format = PixelInternalFormat.R32f;
        heightmaps.create();

        // add separate material for each level
        material_per_level = new Material[lod_levels.count];
        for (var i = lod_levels.lowest.level; i <= lod_levels.highest.level; i++)
            material_per_level[i] = Material.Brass;

        renderer = this.add_vertex_array_renderer();

/*
        tree = new(64, max_level, allocate_chunk);
        var chunk = tree.request_node(0, 0, max_level);
        tree.request_node(-200, 0, max_level);
        tree.request_node(2100, 3200, 0);
*/
        chunks.Clear();

        Garbage.start_measuring(this);
        //query_chunks_within_radius(0, 0, 1024);

        var rect  = new Rectangle((-256, -256), 512);
        var mat = new Material(Material.Pearl);
        mat.ambient_texture = heightmaps;
        var chunk = new TerrainChunk(this, rect, mat); // material_per_level[level]);
        chunks.Add(rect, chunk);
        generate_heightmaps(0, rect);
        chunk.create(priority: 0);


        Garbage.stop_measuring(this);
        heightmaps.query_info();
        //var chunk = chunks.Values.First();
        //this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.vertex_array!, tesselate: false));
        var shader = this.add_shader(Shader.from_file("terrain_shader", "terrain.vert.glsl", "terrain.frag.glsl")).shader;

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

        renderer.render_settings.wireframe    = true;
        renderer.render_settings.depth_test   = true;
        renderer.render_settings.cull_face    = true;
        renderer.render_settings.front_facing = true;

        var total = 0f;

        instance_buffer = new(
                              chunks.Count,
                              new VertexAttribute<InstanceData>(
                                                                "offset",
                                                                2,
                                                                VertexAttribPointerType.Float,
                                                                divisor: 1
                                                               )
                             );

        int index = 0;

        foreach (var c in chunks.Values) {
            instance_buffer[index].offset = (c.rectangle.bottom_left.x, c.rectangle.bottom_left.y);
            instance_buffer[index].size = (short)c.rectangle.width;

            foreach (var vb in c.vertex_array!.vertex_buffers) {
                Console.WriteLine($"{c.rectangle}: {vb.total_size:N0}");
                total += vb.total_size;
            }

            ++index;
        }

        Console.WriteLine($"Chunks: {chunks.Count}, total size: {total:N0}.");

        this.add_behavior(_ => update());
    }

    public void generate_heightmaps(int index, Rectangle area) {
        for (var i = 0; i < heightmaps.width * heightmaps.height; ++i) {
            var px = i % heightmaps.width;
            var py = i / heightmaps.width;

            heightmaps[py, px] =
                noise.sample(
                                    area.left + area.width * px / 96,
                                    area.bottom + area.height * py / 96
                                   );
        }

        heightmaps.update();
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
                    //Debug.println(
                    //              $"Quad at ({center_x},{center_y}), distance={distance:N0}, lod={level}.",
                    //              ConsoleColor.Green
                    //             );
                    var rect  = new Rectangle((x, y), size);
                    var chunk = new TerrainChunk(this, rect, Material.random); // material_per_level[level]);
                    chunks.Add(rect, chunk);
                    generate_heightmaps(chunks.Count - 1, rect);
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