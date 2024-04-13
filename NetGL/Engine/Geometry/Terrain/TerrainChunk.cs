using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public sealed class TerrainChunk {
    private const int chunk_quad_count = 10;
    private const int vertex_count = (chunk_quad_count + 1) * (chunk_quad_count + 1);
    private const int index_count = chunk_quad_count * chunk_quad_count * 2;

    public readonly Terrain terrain;
    public readonly Rectangle rectangle;
    private readonly Material material;

    public bool ready { get; private set; }
    public VertexArrayIndexed? vertex_array { get; private set; }
    public static IndexBuffer<ushort>? shared_index_buffer;

    public TerrainChunk(in Terrain terrain, in Rectangle rectangle, Material material) {
        this.rectangle = rectangle;
        this.terrain = terrain;
        this.ready = false;
        this.material = material;
    }

    private void upload(VertexArrayIndexed va) {
        //Debug.println(Thread.CurrentThread.Name ?? "invalid thread???");
        //Garbage.measure_begin();

        foreach(var b in va.vertex_buffers)
            if (b.status != Buffer.Status.Uploaded)
                b.create();

        if(va.index_buffer.status != Buffer.Status.Uploaded)
            va.index_buffer.create();

        va.upload();

        if (this.vertex_array != null) {
            Debug.assert(false); // TODO: properly remove old vertex array
            terrain.renderer.vertex_arrays.Remove(this.vertex_array!);
        }

        terrain.renderer.vertex_arrays.Add(va, true);

        vertex_array = va;
        ready = true;

        //Garbage.measure("VertexArrayIndexed.upload");
    }

    private IndexBuffer<ushort> get_shared_index_buffer() {
        if (shared_index_buffer is not null) return shared_index_buffer;

        shared_index_buffer = new IndexBuffer<ushort>(index_count);

        int vx, vy;

        var index = 0;
        for (var i = 0; i < index_count / 2; ++i) {
            // Calculate x and y positions based on the current index (i)
            vx = i % chunk_quad_count;
            vy = i / chunk_quad_count;

            var bottom_left  = vy * (chunk_quad_count + 1) + vx;
            var bottom_right = bottom_left + 1;
            var top_left     = bottom_left + chunk_quad_count + 1;
            var top_right    = top_left + 1;

            shared_index_buffer[index].p0 = (ushort)bottom_left;
            shared_index_buffer[index].p1 = (ushort)bottom_right;
            shared_index_buffer[index].p2 = (ushort)top_right;
            ++index;

            shared_index_buffer[index].p0 = (ushort)top_right;
            shared_index_buffer[index].p1 = (ushort)top_left;
            shared_index_buffer[index].p2 = (ushort)bottom_left;
            ++index;
        }

        return shared_index_buffer;
    }

    private VertexArrayIndexed create() {
        var noise = terrain.noise;

        var vb = new VertexBuffer<float3, half3>(
                                         vertex_count);

                                        // new VertexAttribute<float3, half3>("normal", 3, VertexAttribPointerType.HalfFloat)
        var ib = get_shared_index_buffer();

        Debug.println(
                      $"Creating chunk {rectangle.width} x {rectangle.height}, vtx={vertex_count:N0}, idx<{shared_index_buffer?[0].p0.GetType().Name}>={index_count:N0}",
                      ConsoleColor.Magenta
                     );

        float px;
        float py;

        int vx;
        int vy;

        var min_height = float.MaxValue;
        var max_height = float.MinValue;

        // sample some random point to calculate height bias
        float height_bias = 0f;
        for (var i = 0; i < 16; ++i) {
            var x = rectangle.left + rectangle.width * Random.Shared.NextSingle();
            var y = rectangle.bottom + rectangle.height * Random.Shared.NextSingle();
            height_bias += noise.sample(x, y);
        }
        height_bias /= 16;

        for (var i = 0; i < vertex_count; ++i) {
            vx = i % (chunk_quad_count + 1);
            vy = i / (chunk_quad_count + 1);

            px = rectangle.left + rectangle.width * vx / chunk_quad_count;
            py = rectangle.bottom + rectangle.height * vy / chunk_quad_count;

            vb[i].position = terrain.terrain_to_world_position(px,
                                                               py,
                                                               0 //noise.sample(px, py)
                                                              );
            vb[i].position.y -= height_bias;
            if (vb[i].position.y < min_height) min_height = vb[i].position.y;
            if (vb[i].position.y > max_height) max_height = vb[i].position.y;


            // Sample the noise function around the point
            float hL = noise.sample(px - rectangle.width / chunk_quad_count, py);
            float hR = noise.sample(px + rectangle.width / chunk_quad_count, py);
            float hD = noise.sample(px, py - rectangle.width / chunk_quad_count);
            float hU = noise.sample(px, py + rectangle.width / chunk_quad_count);

            // Calculate gradient
            float gradient_x = hR - hL;
            float gradient_y = hU - hD;

            // Construct the normal vector (assuming z is up). Normalize to get unit length.
            var normal = new float3(-gradient_x, -gradient_y, 2 * rectangle.width / chunk_quad_count);
            normal.normalize();
            vb[i].normal = half3.zero; // (half3)normal;

            //heightmap[vx, vy, 0].height = (half)vb[i].position.y;
            //heightmap[vx, vy, 0].normal = vb[i].normal;

        }

        // vb.calculate_normals(ib.indices);

        //Debug.println($"Chunk height range: {min_height:F0}:{max_height:F0}, height_bias: {height_bias:F1}", ConsoleColor.Magenta);

        var va = new VertexArrayIndexed(vb, ib, material);

        return va;
    }

    public void create(int priority) {
        if (priority == 0) {
            upload(create());
        } else {
            BackgroundTaskScheduler.schedule<VertexArrayIndexed>(
                id: $"TerrainChunk.create(rect={rectangle})",
                threaded: create,
                completed: upload,
                priority
            );
        }
    }

    public int key => HashCode.Combine(rectangle.bottom_left, rectangle.top_right);
}