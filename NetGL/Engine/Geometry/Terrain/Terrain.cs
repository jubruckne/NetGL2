using NetGL.Vectors;

namespace NetGL;

using ECS;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
public readonly struct InstanceData {
    public readonly float2 offset;
    public readonly float size;
    public readonly Color color;
    public static implicit operator InstanceData((float x, float y, float size) v) => new(v.x, v.y, v.size);

    public InstanceData(float x, float y, float size) {
        this.offset = (x, y);
        this.size = size;
        color.r = (abs(x) % 100) / 128f;
        color.g = (abs(y) % 100) / 128f;
        color.b = sqrt(size) / 8f;
        color.a = 1f;
    }

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

    private readonly Shader shader;

    private readonly LodLevels lod_levels;

    private readonly VertexBuffer<InstanceData> instance_buffer;
    private readonly VertexBuffer<VertexData> vertex_buffer;
    private readonly IndexBuffer<short> index_buffer;
    private readonly Heightmap heightmap;

    internal Terrain(Entity? parent = null): base("Terrain", parent) {
        camera = get<FirstPersonCamera>(EntityRelationship.HierarchyWithChildrenRecursive);

        //lod_levels = LodLevels.create(6, 16,1);
        lod_levels = LodLevels.create(
                                      [
                                          (1, 16),
                                          (2, 32),
                                          (4, 64),
                                          (8, 128),
                                          (16, 256),
                                          (32, 512),
                                          (64, 768)
                                      ]);

        vertex_buffer = create_vertex_buffer();
        instance_buffer = create_instance_buffer(32000);
        query_chunks_within_radius(get_camera_position(), 4096);
        index_buffer = create_index_buffer();

        Garbage.start_measuring();

        heightmap = new Heightmap(Rectangle.centered_at((0f, 0f), 16384f), 4096 * 2);
        heightmap.update();

        //HeightmapAsset.serialize_to_file(heightmap, "heightmap.jl");
        Garbage.stop_measuring();

        //heightmap = HeightmapAsset.deserialize_from_file("heightmap.jl");


        shader = this.add_shader(Shader.from_file("terrain","terrain.vert.glsl", "terrain.frag.glsl")).shader;
        Materials.Material mat = new("Terrain", shader);
        mat.add_texture("heightmap", heightmap.texture);

        var vertex_array = new VertexArrayIndexed([vertex_buffer, instance_buffer], index_buffer, mat);
        vertex_array.create();
        renderer = this.add_vertex_array_renderer();

       // DrawCommand.List draw_list = new();
        //draw_list.add(new DrawCommand.DrawElementsInstanced(index_buffer, instance_buffer, index_buffer.length, instance_buffer.length));

        renderer.vertex_arrays.Add(vertex_array, true);

        renderer.render_settings.wireframe    = false;
        renderer.render_settings.depth_test   = true;
        renderer.render_settings.cull_face    = true;
        renderer.render_settings.front_facing = true;

        this.add(new Component<Heightmap>(this, "Heightmap", heightmap));

       this.add_behavior(_ => update());
    }

    private VertexBuffer<InstanceData> create_instance_buffer(int max_instances = 16384) {
        var ib = new VertexBuffer<InstanceData>(
                                                max_instances,
                                                VertexAttribute.create_instanced<float2>("offset", divisor: 1),
                                                VertexAttribute.create_instanced<float>("size", divisor: 1),
                                                VertexAttribute.create_instanced<Color>("color", divisor: 1)
                                               );
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

    private void query_chunks_within_radius(float3 position, float radius) {
        var lod = lod_levels.lowest;
        var tile_count = (int)Math.Ceiling(radius / lod.tile_size);

        //Console.WriteLine($"split_chunk: {position}");
        //Console.WriteLine(camera.camera_data.get_view_projection_matrix());


        var center = Rectangle.centered_at(
                                                  float2(
                                                         position.x.nearest_multiple(lod.tile_size),
                                                         position.z.nearest_multiple(lod.tile_size)
                                                        ),
                                                  tile_count * lod.tile_size * 2
                                                 );

        instance_buffer.clear();
        split_chunk(
                    position,
                    (int)radius,
                    center,
                    lod.level
                   );
    }

    private void split_chunk(float3 position,
                             int radius,
                             Rectangle<float> bounds,
                             short level
    ) {
        var size      = lod_levels[level].tile_size;

        ReadOnlySpan<Rectangle<float>> split = bounds.split_by_size(size, size);
        Debug.assert_equal(split.get_area(), bounds.get_area());
        Debug.assert_equal(bounds.left, split[0].left);
        Debug.assert_equal(bounds.bottom, split[0].bottom);
        Debug.assert_equal(bounds.right, split[^1].right);
        Debug.assert_equal(bounds.top, split[^1].top);

        foreach (var rect in split) {
            var distance = position.distance_to((rect.center.x, 0, rect.center.y)) - size / 2f;

            if (distance <= lod_levels[level].max_distance && level < lod_levels.max_level) {
                split_chunk(position, radius, rect, (short)(level + 1).at_most(lod_levels.max_level));
                //Console.WriteLine();
            } else if (distance <= radius) {
                if(camera.frustum.intersects(Box.from_rectangle_xz(rect))) {
                    //Console.WriteLine(
                    //                  $"{new string(' ', level * 2)}chunk: rect={rect}, center={rect.center}, rect_size={rect.width} size={size}, level={level}"
                    //                 );
                    var instance = new InstanceData(
                                                    x: rect.center.x,
                                                    y: rect.center.y,
                                                    size: size
                                                   );
                    instance_buffer.append(instance);
                }
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
        var pos    = get_camera_position();
        query_chunks_within_radius(pos, 4096);
        instance_buffer.update();

        //Console.WriteLine($"# of chunks: {chunks.Length}, world_pos={pos}");


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