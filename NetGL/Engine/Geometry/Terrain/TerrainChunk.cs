using NetGL.Debug;
using OpenTK.Mathematics;

namespace NetGL;

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

    private static readonly Dictionary<int, IIndexBuffer> index_buffer_per_resolution = new ();

    public TerrainChunk(in Terrain terrain, in Key key) {
        this.key = key;
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

        if (this.vertex_array != null) {
            terrain.renderer.vertex_arrays.Remove(this.vertex_array!);
        }

        terrain.renderer.vertex_arrays.Add(va);

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
        Console.WriteLine("before create: "); Garbage.log();

        var gen = generate() as IShapeGenerator2;
        Console.WriteLine("shape gen: "); Garbage.log();


        VertexBuffer.Position_Normal<Vector3, Vector3> vb = new(); //.get_vertex_count()); //.get_vertices_and_normals(), VertexAttribute.Position, VertexAttribute.Normal);
        IndexBuffer<int> ib = new();

        gen.fill(vb.normals, vb.positions, ib.indices);


        /*
        lock (index_buffer_per_resolution) {
            if (!index_buffer_per_resolution.TryGetValue(resolution, out ib)) {
                ib = IndexBuffer.create(gen.get_indices(), vb.count);
                index_buffer_per_resolution.Add(resolution, ib);
            }
        }
*/
        Console.WriteLine("vertex buffer gen: "); Garbage.log();



        Console.WriteLine("index buffer gen: "); Garbage.log();


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