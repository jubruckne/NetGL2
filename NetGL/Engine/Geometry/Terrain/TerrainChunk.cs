namespace NetGL;

internal sealed class TerrainChunk {
    private const int chunk_quad_count = 255;
/*
    internal readonly record struct Key(short x, short y) {
        public IEnumerable<Key> neighbors(byte distance = 1) {
            // Top and bottom horizontal lines
            for (var dx = -distance; dx <= distance; dx++) {
                yield return new Key((short)(x + dx), (short)(y + distance)); // Top
                yield return new Key((short)(x + dx), (short)(y - distance)); // Bottom
            }

            // Left and right vertical lines, excluding corners which are already added
            for (var dy = -distance + 1; dy <= distance - 1; dy++) {
                yield return new Key((short)(x + distance), (short)(y + dy)); // Right
                yield return new Key((short)(x - distance), (short)(y + dy)); // Left
            }
        }

        public static Key from_world_position(in Terrain terrain, in Vector3 world_position) {
            var position = terrain.plane.world_to_point_on_plane_2d(world_position);
            var x = (short)Math.Floor(position.X / terrain.chunk_size);
            var y = (short)Math.Floor(position.Y / terrain.chunk_size);
            return new Key(x, y);
        }

        public static Vector3 to_world_position(in Terrain terrain, in Key key, float height = 0f) {
            var p = to_terrain_position(terrain, key);
            return terrain.plane.to_world(p.X, p.Y, height);
        }

        public static Vector2 to_terrain_position(in Terrain terrain, in Key key) {
            return new Vector2(
                (key.x - 0.5f) * terrain.chunk_size,
                (key.y - 0.5f) * terrain.chunk_size);
        }
    }
*/
    public readonly Terrain terrain;
    public readonly float x;
    public readonly float y;
    public readonly float size;

    public bool ready { get; private set; }
    public VertexArrayIndexed? vertex_array { get; private set; }

    public TerrainChunk(in Terrain terrain, float x, float y, float size) {
        this.x = x;
        this.y = y;
        this.size = size;
        this.terrain = terrain;
        this.ready = false;
    }

    private void upload(VertexArrayIndexed va) {
        Debug.println(Thread.CurrentThread.Name ?? "invalid thread???");
        //Garbage.measure_begin();

        foreach(var b in va.vertex_buffers)
            if (b.status != Buffer.Status.Uploaded)
                b.upload();

        if(va.index_buffer.status != Buffer.Status.Uploaded)
            va.index_buffer.upload();

        va.upload();

        if (this.vertex_array != null) {
            terrain.renderer.vertex_arrays.Remove(this.vertex_array!);
        }

        terrain.renderer.vertex_arrays.Add(va);

        vertex_array = va;
        ready = true;

        //Garbage.measure("VertexArrayIndexed.upload");
    }

    private VertexArrayIndexed create() {
        Garbage.measure_begin();

        int vertex_count = (chunk_quad_count + 1) * (chunk_quad_count + 1);
        int index_count  = chunk_quad_count * chunk_quad_count * 2;

        Plane plane = terrain.plane;
        Noise noise = terrain.noise;

        var vb           = new VertexBuffer<float3, half3>(vertex_count);
        var ib           = new IndexBuffer<ushort>(index_count);
        var triangulator = new Triangulator<float3, ushort>(vb.positions, ib.indices);

        Debug.println(
                      $"Creating chunk {size} x {size}, vtx={vertex_count:N0}, idx<{ib[0].p0.GetType().Name}>={index_count:N0}",
                      ConsoleColor.Magenta
                     );

        float px;
        float py;

        int vx;
        int vy;

        int index = 0;
        for (var i = 0; i < vertex_count; ++i) {
            vx = i % (chunk_quad_count + 1);
            vy = i / (chunk_quad_count + 1);

            Console.WriteLine($"{vx}, {vy}");

            px = this.x + size * vx / chunk_quad_count;
            py = this.y + size * vy / chunk_quad_count;

            vb[i].position = plane.to_world(
                                            px,
                                            py,
                                            noise.sample(px, py)
                                           );

            if (vx < chunk_quad_count && vy < chunk_quad_count) {
                var top_right    = (vy + 1) * (chunk_quad_count + 1) + vx + 1;
                var bottom_right = vy * (chunk_quad_count + 1) + vx + 1;
                var bottom_left  = vy * (chunk_quad_count + 1) + vx;
                var top_left     = (vy + 1) * (chunk_quad_count + 1) + vx;

                ib[index].p0 = (ushort)bottom_left;
                ib[index].p1 = (ushort)bottom_right;
                ib[index].p2 = (ushort)top_right;
                ++index;

                ib[index].p0 =  (ushort)top_right;
                ib[index].p1 =  (ushort)top_left;
                ib[index].p2 =  (ushort)bottom_left;
                ++index;
            }
        }

        vb.calculate_normals(ib.indices);

        var va = new VertexArrayIndexed(vb, ib);

        Garbage.measure("TerrainChunk.create");

        return va;
    }

    public void create(int priority) {
        if (priority == 0) {
            upload(create());
        } else {
            BackgroundTaskScheduler.schedule<VertexArrayIndexed>(
                id: $"TerrainChunk.create(pos={(x, y)}, size={size})",
                threaded: create,
                completed: upload,
                priority
            );
        }
    }
}