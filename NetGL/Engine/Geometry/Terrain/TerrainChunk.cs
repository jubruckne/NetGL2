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

    private static readonly IndexBuffer<ushort>? common_index_buffer = null;

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

        int       width          = terrain.chunk_size * resolution / 16;
        int       height         = terrain.chunk_size * resolution / 16;
        const int segment_height = 32;
        int vertex_count = (width + 1) * (height + 1) + (segment_height * width * 8);
        int       index_count    = (width + 1) * (height + 1) * 2;
        int       chunk_size     = terrain.chunk_size;
        Plane     plane          = terrain.plane;
        Noise     noise          = terrain.noise;
        Vector2   center         = this.center;

        Console.WriteLine($"Creating chunk {width} x {height}");

        var vb           = new VertexBuffer<Vector3, Vector3>(vertex_count);
        var ib           = new IndexBuffer<ushort>(index_count);
        var triangulator = new Triangulator<Vector3, ushort>(vb.positions, ib.indices);

        float px0, px1;
        float py0, py1;

        for (var y = 0; y <= height; ++y) {
            //Console.WriteLine($"y = {y}");
            py0 = center.Y + chunk_size * (float)y / height;
            py1 = center.Y + chunk_size * (float)(y+1) / height;

            for (var x = 0; x <= width; ++x) {
                px0 = center.X + chunk_size * (float)x / width;
                px1 = center.X + chunk_size * (float)(x+1) / width;
                triangulator.quad(
                                  plane.to_world(
                                                 px0,
                                                 py0,
                                                 noise.sample(px0, py0)
                                                ),
                                  plane.to_world(
                                                 px1,
                                                 py0,
                                                 noise.sample(px1, py0)
                                                ),
                                  plane.to_world(
                                                 px1,
                                                 py1,
                                                 noise.sample(px1, py1)
                                                ),
                                  plane.to_world(
                                                 px0,
                                                 py1,
                                                 noise.sample(px0, py1)
                                                )
                                 );
            }
        }

        vb.calculate_normals(ib.indices);

        var va = new VertexArrayIndexed(vb, ib, triangulator.finish());

        Garbage.measure("TerrainChunk.create");

        return va;
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