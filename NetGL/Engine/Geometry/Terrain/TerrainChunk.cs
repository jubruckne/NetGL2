using NetGL.Debug;

namespace NetGL;

using OpenTK.Mathematics;

internal sealed class TerrainChunk : IShape {
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
            var (position, _) = terrain.plane.world_to_point_on_plane(world_position);
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

    public readonly Key key;
    public readonly Terrain terrain;

    public Vector2 center => Key.to_terrain_position(terrain, key);

    public int resolution { get; private set; }
    public bool ready { get; private set; }
    public VertexArrayIndexed? vertex_array { get; private set; }

    private static readonly Dictionary<int, IndexBuffer<int>> index_buffer_per_resolution = new ();

    public TerrainChunk(in Terrain terrain, in Key key) {
        this.key = key;
        this.terrain = terrain;
        this.ready = false;
    }

    private void upload(VertexArrayIndexed va) {
        Console.WriteLine(Thread.CurrentThread.Name);
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

    public void update(int new_resolution) {
        if (ready) {
            ready = false;
            terrain.renderer.vertex_arrays.Remove(vertex_array!);
            this.vertex_array = null;
        }

        create(new_resolution, 0);
    }

    private VertexArrayIndexed create() {
        Garbage.measure_begin();

        var gen = generate();

        int pixel_count = terrain.chunk_size * resolution / 16;
        int vertex_count = (pixel_count + 1) * (pixel_count + 1);
        int index_count = pixel_count * pixel_count * 2;

        var vb = new VertexBuffer<(Vector3 position, Vector3 normal)>(vertex_count,
            VertexAttribute.Position,
            VertexAttribute.Normal
        );

        var vertex_writer = vb.get_writer<Vector3>("position");

        float px = 0;
        float py = 0;

        for (var x = 0; x < pixel_count + 1; x++) {
            px = center.X + terrain.chunk_size * (float)x / pixel_count;
            for (var y = 0; y < pixel_count + 1; y++) {
                py = center.Y + terrain.chunk_size * (float)y / pixel_count;
                vertex_writer.write(terrain.plane.to_world(
                    px,
                    py,
                    terrain.noise.sample(px, py)));
            }
        }

        Error.assert(vertex_writer.eof);

        IndexBuffer<int> ib;

        lock (index_buffer_per_resolution) {
            if (!index_buffer_per_resolution.TryGetValue(resolution, out ib!)) {
                ib = new IndexBuffer<int>(index_count);
                var index_writer = ib.get_writer<int>();

                for (var x = 1; x < pixel_count + 1; ++x) {
                    for (var y = 1; y < pixel_count + 1; ++y) {
                        var bottom_left = (y - 1) + (x - 1) * (pixel_count + 1);
                        var bottom_right = (y - 1) + x * (pixel_count + 1);
                        var top_left = y + (x - 1) * (pixel_count + 1);
                        var top_right = y + x * (pixel_count + 1);

                        index_writer.write(bottom_left, bottom_right, top_left);
                        index_writer.write(top_left, bottom_right, top_right);
                    }
                }

                Error.assert(index_writer, index_writer.eof);

                index_buffer_per_resolution.Add(resolution, ib);
            }
        }

        var vert = vb.get_view();
        foreach (var tri in ib.get_view()) {
            var edge1 = vert[tri.p2].position - vert[tri.p1].position;
            var edge2 = vert[tri.p3].position - vert[tri.p1].position;
            var normal = Vector3.Cross(edge1, edge2);

            vert[tri.p1].normal += normal;
            vert[tri.p2].normal += normal;
            vert[tri.p3].normal += normal;
        }

        for (var i = 0; i < vert.length; i++) {
            vert[i].normal.Normalize();
        }

        Garbage.measure("TerrainChunk.create");

        return new VertexArrayIndexed(ib, vb);
    }

    public void create(int resolution, int priority) {
        this.resolution = resolution;

        if (priority == 0) {
            upload(create());
        } else {
            BackgroundTaskScheduler.schedule<VertexArrayIndexed>(
                id: $"TerrainChunk.create(pos={key.x},{key.y}, res={resolution})",
                threaded: create,
                completed: upload,
                priority
            );
        }
    }

    public IShapeGenerator generate() => new TerrainShapeGenerator(this);
}