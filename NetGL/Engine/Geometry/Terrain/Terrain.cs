namespace NetGL;

using ECS;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
public struct InstanceData {
    public short2 offset;
    public half size;
    public static implicit operator InstanceData((short x, short y, half size) v) => new() { offset = (v.x, v.y), size = v.size };

    public override string ToString()
        => $"offset={offset}, size={size}";
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
public struct VertexData {
    public byte x;
    public byte z;
    public static implicit operator VertexData((byte x, byte z) v) => new() { x = v.x, z = v.z };
}

public class Terrain: Entity {
    public readonly VertexArrayRenderer renderer;

    //[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    //readonly Quadtree<TerrainChunk> tree;

    private readonly FirstPersonCamera camera;

    internal readonly TerrainNoise noise;
    private readonly Shader shader;

    private readonly Material[] material_per_level;
    private readonly Dictionary<Rectangle, TerrainChunk> chunks = new();
    private readonly LodLevels lod_levels;

    private readonly VertexBuffer<InstanceData> instance_buffer;
    private readonly VertexBuffer<VertexData> vertex_buffer;
    private readonly IndexBuffer<short> index_buffer;
    private readonly Heightmap heightmap;

    internal Terrain(Entity? parent = null): base("Terrain", parent) {
        const int detail_levels = 1;
        const int tile_size_at_highest_level = 512;

        camera = get<FirstPersonCamera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();

        lod_levels = LodLevels.create(detail_levels, tile_size_at_highest_level);


        vertex_buffer = create_vertex_buffer();
        instance_buffer = create_instance_buffer();
        index_buffer = create_index_buffer();
        heightmap = create_heightmap();
        HeightmapAsset.serialize_to_file(heightmap, "heightmap.jl");


        shader = this.add_shader(Shader.from_file("terrain","terrain.vert.glsl", "terrain.frag.glsl")).shader;
        Materials.Material mat = new("Terrain", shader);
        mat.add_texture("heightmap", heightmap.texture);

        var vertex_array = new VertexArrayIndexed([vertex_buffer, instance_buffer], index_buffer, mat);
        vertex_array.create();
        renderer = this.add_vertex_array_renderer();


        renderer.vertex_arrays.Add(vertex_array, true);
/*
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
        //heightmaps.create();

        // add separate material for each level
        material_per_level = new Material[lod_levels.count];
        for (var i = lod_levels.lowest.level; i <= lod_levels.highest.level; i++)
            material_per_level[i] = Material.Brass;

        renderer = this.add_vertex_array_renderer();

        chunks.Clear();

        Garbage.start_measuring(this);
        //query_chunks_within_radius(0, 0, 1024);

        shader = this.add_shader(Shader.from_file("name","terrain.vert.glsl", "terrain.frag.glsl", "terrain.tessctrl.glsl", "terrain.tesseval.glsl")).shader;//, "terrain.frag.glsl", tess_control: "", tess_eval: "terrain.tesseval.glsl")).shader;

        var rect  = new Rectangle((-256, -256), 512);
        Materials.Material mat = new("Terrain", shader);
       // mat.add_texture("heightmap", heightmaps);
       // mat.add_color("tile_color", Color.White);

        var chunk = new TerrainChunk(this, rect, mat); // material_per_level[level]);
        chunks.Add(rect, chunk);
        generate_heightmaps(0, rect);
        chunk.create(priority: 0);

        VertexArray.unbind();


        Garbage.stop_measuring(this);
        //heightmaps.query_info();
        //var chunk = chunks.Values.First();
        //this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunk.vertex_array!, tesselate: false));

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

        /*
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
*/
        //this.add_behavior(_ => update());
    }

    private Heightmap create_heightmap(int texture_size = 4096, int world_size = 4096) {
        var map = new Heightmap(texture_size,
                                Rectangle.centered_at((0, 0), world_size)
                               );

        var area = new Rectangle(0, 0, world_size, world_size);

        for (var i = 0; i < texture_size * texture_size; ++i) {
            var px = i % texture_size;
            var py = i / texture_size;

            map.texture[py, px] =
                noise.sample(
                             area.left + area.width * px / world_size,
                             area.bottom + area.height * py / world_size
                            );
        }

        map.texture.create();

        return map;
    }

    private VertexBuffer<InstanceData> create_instance_buffer(int tile_count = 32, int tile_size = 10, float world_size = 100) {
        var ib = new VertexBuffer<InstanceData>(
                                                tile_count * tile_count,
                                                VertexAttribute.short2("offset", divisor: 1),
                                                VertexAttribute.scalar<half>("size", divisor: 1)
                                               );

        for(var x = 0; x < tile_count; x++) {
            for(var y = 0; y < tile_count; y++) {
                ib[x * tile_count + y] = ((short)(x * world_size), (short)(y * world_size),
                                          (half)(1.0f / tile_size * world_size));
            }
        }

        ib.create();
        return ib;
    }

    private IndexBuffer<short> create_index_buffer(byte tile_size = 10) {
        var ib = new IndexBuffer<short>(tile_size * tile_size * 2);

        var pos = 0;

        for (byte i = 0; i < tile_size; i++) {
            for (byte j = 0; j < tile_size; j++) {
                // Add the first triangle
                ib[pos].p2 = (short)(i * (tile_size + 1) + j);
                ib[pos].p1 = (short)((i + 1) * (tile_size + 1) + j);
                ib[pos].p0 = (short)(i * (tile_size + 1) + j + 1);
                ++pos;

                // Add the second triangle
                ib[pos].p2 = (short)(i * (tile_size + 1) + j + 1);
                ib[pos].p1 = (short)((i + 1) * (tile_size + 1) + j);
                ib[pos].p0 = (short)((i + 1) * (tile_size + 1) + j + 1);
                ++pos;
            }
        }

        Console.WriteLine($"Index Buffer created: length={ib.length}, max_vtx={ib.calculate_vertex_range()}");

        ib.create();
        return ib;
    }

    private VertexBuffer<VertexData> create_vertex_buffer(byte tile_size = 10) {
        var vb = new VertexBuffer<VertexData>(
                                              (tile_size + 1) * (tile_size + 1),
                                              VertexAttribute.buffer<VertexData>(
                                                   "position",
                                                   VertexAttribPointerType.Byte,
                                                   2
                                                  )
                                             );


        var pos = 0;
        for (byte i = 0; i <= tile_size; i++) {
            for (byte j = 0; j <= tile_size; j++) {
                vb[pos++] = (i, j);
            }
        }

        vb.create();

        return vb;
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
                    /*
                    var rect  = new Rectangle((x, y), size);
                    Materials.Material mat = new("Terrain", this.shader);
                    mat.add_texture("heightmap", heightmaps);
                    mat.add_color("tile_color", Color.random_for(rect));

                    var chunk = new TerrainChunk(this, rect, mat); // material_per_level[level]);
                    chunks.Add(rect, chunk);
                    generate_heightmaps(chunks.Count - 1, rect);
                    chunk.create(priority: 0);
                    */
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