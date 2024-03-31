namespace NetGL;

internal sealed class TerrainChunk {
    private const int chunk_quad_count = 255;

    public readonly Terrain terrain;
    public readonly float x;
    public readonly float y;
    public readonly float size;

    public bool ready { get; private set; }
    public VertexArrayIndexed? vertex_array { get; private set; }

    public TerrainChunk(in Terrain terrain, in Bounds bounds) {
        this.x = bounds.center.x;
        this.y = bounds.center.y;
        this.size = bounds.size;
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

        terrain.renderer.vertex_arrays.Add(va, true);

        vertex_array = va;
        ready = true;

        //Garbage.measure("VertexArrayIndexed.upload");
    }

    private VertexArrayIndexed create() {
        using Garbage g = new();

        int vertex_count = (chunk_quad_count + 1) * (chunk_quad_count + 1);
        int index_count  = chunk_quad_count * chunk_quad_count * 2;

        Plane plane = terrain.plane;
        Noise noise = terrain.noise;

        var vb           = new VertexBuffer<float3, half3>(vertex_count);
        var ib           = new IndexBuffer<ushort>(index_count);

        Debug.println(
                      $"Creating chunk {size} x {size}, vtx={vertex_count:N0}, idx<{ib[0].p0.GetType().Name}>={index_count:N0}",
                      ConsoleColor.Magenta
                     );

        var offset_x = this.x - this.size * 0.5f;
        var offset_y = this.y - this.size * 0.5f;

        float px;
        float py;

        int vx;
        int vy;

        var index = 0;
        for (var i = 0; i < vertex_count; ++i) {
            vx = i % (chunk_quad_count + 1);
            vy = i / (chunk_quad_count + 1);

            px = offset_x + size * vx / chunk_quad_count;
            py = offset_y + size * vy / chunk_quad_count;

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