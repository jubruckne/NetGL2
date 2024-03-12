using OpenTK.Mathematics;

namespace NetGL;

internal class TerrainChunk: IShape {
    public readonly Terrain terrain;

    public readonly int x;
    public readonly int y;
    public int resolution { get; private set; }
    public bool ready { get; private set; }
    public VertexArrayIndexed? vertex_array { get; private set; }

    private static readonly Dictionary<int, IIndexBuffer> index_buffer_per_resolution = new ();

    public TerrainChunk(Terrain terrain, int x, int y) {
        this.x = x;
        this.y = y;
        this.terrain = terrain;
        this.ready = false;
    }

    private void upload(VertexArrayIndexed va) {
        foreach(var b in va.vertex_buffers)
            if (b.status != Buffer.Status.Uploaded)
                b.upload();

        if(va.index_buffer.status != Buffer.Status.Uploaded)
            va.index_buffer.upload();

        va.upload();

        vertex_array = va;
        ready = true;
    }

    public void update(int new_resolution) {
        if (ready) {
            ready = false;
            terrain.renderer.vertex_arrays.Remove(vertex_array!);
            this.vertex_array = null;
        }

        create(new_resolution, 0);
    }

    private VertexArrayIndexed create() {
        var gen = generate();
        VertexBuffer<Struct<Vector3, Vector3>> vb = new(gen.get_vertices_and_normals(), VertexAttribute.Position, VertexAttribute.Normal);

        if (!index_buffer_per_resolution.TryGetValue(resolution, out var ib)) {
            ib = IndexBuffer.create(gen.get_indices(), vb.count);
            index_buffer_per_resolution.Add(resolution, ib);
        }

        return new VertexArrayIndexed(ib, vb);
    }

    public void create(int resolution, int priority) {
        this.resolution = resolution;

        if (priority == 0) {
            upload(create());
        } else {
            BackgroundTaskScheduler.schedule<VertexArrayIndexed>(
                id: $"TerrainChunk.create(pos={x},{y}, res={resolution})",
                threaded: create,
                completed: upload,
                priority
            );
        }
    }

    public IShapeGenerator generate() => new TerrainShapeGenerator(this);
}