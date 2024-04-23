using NetGL.Vectors;

namespace NetGL;

using ECS;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
public struct InstanceData {
    public short2 offset;
    public half size;
    public static implicit operator InstanceData((short x, short y, half size) v) => new() { offset = (v.x, v.y), size = v.size };
    public static implicit operator InstanceData((short x, short y, float size) v) => new() { offset = (v.x, v.y), size = (half)v.size };

    public override string ToString()
        => $"offset={offset}, size={size}";
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public struct VertexData {
    public half x;
    public half z;

    public static implicit operator VertexData((half x, half z) v) => new() { x = v.x, z = v.z };
}

public class Terrain: Entity {
    public readonly VertexArrayRenderer renderer;

    private readonly FirstPersonCamera camera;

    internal readonly TerrainNoise noise;
    private readonly Shader shader;

    private readonly LodLevels lod_levels;

    private readonly VertexBuffer<InstanceData> instance_buffer;
    private readonly VertexBuffer<VertexData> vertex_buffer;
    private readonly IndexBuffer<short> index_buffer;
    private readonly Heightmap heightmap;

    internal Terrain(Entity? parent = null): base("Terrain", parent) {
        camera = get<FirstPersonCamera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();

        lod_levels = LodLevels.create(4, 16,1);

        vertex_buffer = create_vertex_buffer();
        instance_buffer = create_instance_buffer(query_chunks_within_radius(get_camera_position(), 24));
        index_buffer = create_index_buffer();

        Garbage.start_measuring(this);

        heightmap = new Heightmap(1024, Rectangle.centered_at((0, 0), 4096));
        heightmap.generate(noise);
        //HeightmapAsset.serialize_to_file(heightmap, "heightmap.jl");

        //heightmap = HeightmapAsset.deserialize_from_file("heightmap.jl");

        Garbage.stop_measuring(this);

        shader = this.add_shader(Shader.from_file("terrain","terrain.vert.glsl", "terrain.frag.glsl")).shader;
        Materials.Material mat = new("Terrain", shader);
        mat.add_texture("heightmap", heightmap.texture);

        var vertex_array = new VertexArrayIndexed([vertex_buffer, instance_buffer], index_buffer, mat);
        vertex_array.create();
        renderer = this.add_vertex_array_renderer();

        renderer.vertex_arrays.Add(vertex_array, true);

        renderer.render_settings.wireframe    = true;
        renderer.render_settings.depth_test   = true;
        renderer.render_settings.cull_face    = true;
        renderer.render_settings.front_facing = true;

        this.add_behavior(_ => update());
    }

    private VertexBuffer<InstanceData> create_instance_buffer(Span<InstanceData> instances) {
        var ib = new VertexBuffer<InstanceData>(
                                                instances.Length,
                                                VertexAttribute.create_instanced<short2>("offset", divisor: 1),
                                                VertexAttribute.create_instanced<half>("size", divisor: 1)
                                               );

        for (var index = 0; index < instances.Length; index++)
            ib[index] = instances[index];

        ib.create();
        return ib;
    }

    private IndexBuffer<short> create_index_buffer(byte tile_count = 10) {
        var ib = new IndexBuffer<short>(tile_count * tile_count * 2);

        var pos = 0;

        for (byte i = 0; i < tile_count; i++) {
            for (byte j = 0; j < tile_count; j++) {
                // Add the first triangle
                ib[pos].p2 = (short)(i * (tile_count + 1) + j);
                ib[pos].p1 = (short)((i + 1) * (tile_count + 1) + j);
                ib[pos].p0 = (short)(i * (tile_count + 1) + j + 1);
                ++pos;

                // Add the second triangle
                ib[pos].p2 = (short)(i * (tile_count + 1) + j + 1);
                ib[pos].p1 = (short)((i + 1) * (tile_count + 1) + j);
                ib[pos].p0 = (short)((i + 1) * (tile_count + 1) + j + 1);
                ++pos;
            }
        }

        Console.WriteLine($"Index Buffer created: length={ib.length}, max_vtx={ib.calculate_vertex_range()}");

        ib.create();
        return ib;
    }

    private VertexBuffer<VertexData> create_vertex_buffer(int tile_count = 10, float width = 1f, float height = 1f) {
        var vb = new VertexBuffer<VertexData>(
                                              (tile_count + 1) * (tile_count + 1),
                                              VertexAttribute.create<VertexData>("position")
                                             );


        var half_width = width / 2f;
        var half_height = height / 2f;

        var pos = 0;
        for (var i = 0; i <= tile_count; i++) {
            for (var j = 0; j <= tile_count; j++) {
                vb[pos++] = ((half)(i * width / tile_count - half_width), (half)(j * height / tile_count - half_height));
            }
        }

        vb.create();

        return vb;
    }

    private Span<InstanceData> query_chunks_within_radius(float3 position, float radius) {
        var lod = lod_levels.lowest;
        var tile_count = (int)Math.Ceiling(radius / lod.tile_size);

        //Console.WriteLine($"checking for chunks to render with maxsize = {lod.size}, {lod}");

        var center = Rectangle<int>.centered_at(
                                                int2(
                                                     (int)position.x.nearest_multiple(lod.tile_size),
                                                     (int)position.z.nearest_multiple(lod.tile_size)
                                                    ),
                                                tile_count * lod.tile_size * 2
                                               );

        var chunks = new List<InstanceData>();
        split_chunk(
                    chunks,
                    position,
                    (int)radius,
                    center,
                    lod.level
                   );
        return chunks.ToArray();
    }

    private void split_chunk(List<InstanceData> chunks,
                             float3 position,
                             int radius,
                             Rectangle<int> bounds,
                             short level
    ) {
        var size      = lod_levels[level].tile_size;

        foreach (var rect in bounds.split_by_size(size, size)) {
            var distance = position.distance_to((rect.center.x, 0, rect.center.y));

            if (distance <= lod_levels[level].max_distance && level < lod_levels.max_level) {
                split_chunk(chunks, position, radius, rect, (short)(level + 1).at_most(lod_levels.max_level));
            } else if (distance <= radius) {
                Console.WriteLine($"{new string(' ', level * 2)}chunk: rect={rect}, center={rect.center}, rect_size={rect.width} size={size}, level={level}");
                var instance = new InstanceData {
                                                    offset = ((short)rect.center.x, (short)rect.center.y),
                                                    size = (half)size
                                                };
                chunks.Add(instance);
            }
        }
/*
        for (var x = bottom_left.x + half_size; x < top_right.x + half_size; x += size) {
            for (var y = bottom_left.y + half_size; y < top_right.y + half_size; y += size) {
                Console.WriteLine($"split_chunk: x={x}, y={y}, size={size}, level={level}");
                var distance = (int)MathF.Sqrt(
                                               MathF.Pow(x - position.x, 2)
                                               + MathF.Pow(y - position.y, 2)
                                              );

                /*var angle_to_chunk = MathF.Atan2(center_y - position.y, center_x - position.x);

                var isWithinFoV = MathF.Abs(0 - angle_to_chunk)
                                  <= camera.field_of_view_degrees.degree_to_radians() * 0.55f; // allow 10% margin
*/
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float3 get_camera_position()
        => camera.transform.position;

    private void update() {
        /*var (terrain_x, terrain_y, terrain_height) = world_to_terrain_position();
        var chunks = query_chunks_within_radius((terrain_x, terrain_y), 200);
        Console.WriteLine($"terrain_pos={terrain_x:N2}:{terrain_y:N2}, height={terrain_height}, chunks={chunks.Length}");
        for (var i = 0; i < chunks.Length; i++)
            instance_buffer[i] = chunks[i];
        instance_buffer.update();
*/
        /*
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