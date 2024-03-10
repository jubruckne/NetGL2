using NetGL.ECS;
using OpenTK.Mathematics;

namespace NetGL;

internal class TerrainChunk: IShape<TerrainChunk> {
    public readonly struct Key {
        public readonly Vector2i center;
        public readonly int resolution;

        public Key(Vector2i center, int resolution) {
            this.center = center;
            this.resolution = resolution;
        }

        public static implicit operator Key((int x, int y, int resolution) key) {
            return new((key.x, key.y), key.resolution);
        }

        public override string ToString() => $"({center.X},{center.Y}:{resolution})";
    }

    public readonly Terrain terrain;
    public readonly Vector2i center;
    public readonly int size;
    public readonly int resolution;
    public VertexArrayIndexed? vertex_array;
    public readonly Key key;

    private static readonly Dictionary<int, IIndexBuffer> index_buffer_per_resolution = new ();

    public TerrainChunk(Terrain terrain, in Vector2i center, int size, int resolution) {
        this.key = new Key(center, resolution);
        this.terrain = terrain;
        this.center = center;
        this.size = size;
        this.resolution = resolution;
    }

    public void upload() {
        if (vertex_array == null) throw new Exception("Vertex array not created!");

        foreach(var b in vertex_array.vertex_buffers)
            if (b.status != Buffer.Status.Uploaded)
                b.upload();

        if(vertex_array.index_buffer.status != Buffer.Status.Uploaded)
            vertex_array.index_buffer.upload();

        vertex_array.upload();
    }

    public void create() {
        var gen = generate();
        VertexBuffer<Struct<Vector3, Vector3>> vb = new(gen.get_vertices_and_normals(), VertexAttribute.Position, VertexAttribute.Normal);

        if (!index_buffer_per_resolution.TryGetValue(resolution, out var ib)) {
            ib = IndexBuffer.create(gen.get_indices(), vb.count);
            index_buffer_per_resolution.Add(resolution, ib);
        }

        vertex_array = new VertexArrayIndexed(ib, vb);
    }

    public IShapeGenerator generate(TerrainShapeGenerator.Options options) => new TerrainShapeGenerator(this, options);
    public IShapeGenerator generate() => new TerrainShapeGenerator(this);
}

public static class TerrainFab {
    public static Terrain create_terrain(this World world, in Plane plane) {
        var e = new Terrain(plane, world);
        world.add_entity(e);
        return e;
    }
}

public class Terrain : Entity {
    public readonly Plane plane;
    public readonly Material material;
    public readonly VertexArrayRenderer renderer;
    public readonly ShaderComponent shader;

    public const int max_resolution = 32;
    public readonly int chunk_size = 100;

    private readonly Dictionary<TerrainChunk.Key, TerrainChunk> chunks;
    private readonly Camera camera;

    internal readonly Noise noise;

    internal Terrain(Plane plane, Entity? parent = null) : base("Terrain", parent) {
        this.plane = plane;

        camera = get<Camera>(EntityRelationship.HierarchyWithChildrenRecursive);

        noise = new();
        noise.add_simplex_layer(0.001f, 35f);
        noise.add_simplex_layer(0.002f, 35f);
        noise.add_simplex_layer(0.01f, 2.5f);
        noise.add_value_layer(2.5f, 0.035f);

        chunks = new Dictionary<TerrainChunk.Key, TerrainChunk>();

        material = this.add_material(Material.random).material;
        renderer = this.add_vertex_array_renderer();
        request_chunk((0, 0, max_resolution));
        shader = this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunks[(0, 0, max_resolution)].vertex_array,
            material));
        renderer.wireframe = true;
        this.add_behavior(_ => update());
    }

    public float get_height_at_world_position(Vector3 world_position) {
        var (tp, _) = plane.world_to_point_on_plane(world_position);

        return noise.sample(tp);
    }

    private void update() {
        var (position, height) = plane.world_to_point_on_plane(camera.transform.position);

        var h = get_height_at_world_position(camera.transform.position) + 8f;
        if (camera.transform.position.Y < h) {
            camera.transform.position.Y = float.Lerp(camera.transform.position.Y, h, 0.09f);
        } else if (camera.transform.position.Y > h) {
            camera.transform.position.Y = float.Lerp(h, camera.transform.position.Y, 0.12f);
        }

        int x = (int)(Math.Round(position.X / chunk_size) * chunk_size);
        int y = (int)(Math.Round(position.Y / chunk_size) * chunk_size);
        //Console.WriteLine($"Terrain: chunk {position}, reverse: {plane.to_world(position, height)}, cam at: {camera.transform.position}");

        request_chunk((x, y, max_resolution));
        /*
        else {
            if (chunks[(x, y)].resolution != max_resolution) {
                renderer.vertex_arrays.Remove(chunks[(x, y)].vertex_array);
                chunks[(x, y)].resolution = max_resolution;
                chunks[(x, y)].update();
                renderer.vertex_arrays.Add(chunks[(x, y)].vertex_array);
            }
        }
        */
    }

    private void request_chunk(in TerrainChunk.Key key) {
        if (!chunks.ContainsKey(key)) {
            Console.WriteLine($"generating chunk {key.center}, resolution = {key.resolution}...");
            var chunk = new TerrainChunk(this, key.center, chunk_size, key.resolution);
            chunk.create();
            chunk.upload();
            chunks.Add(key, chunk);
            renderer.vertex_arrays.Add(chunk.vertex_array!);
        }

        for (int distance = 1; distance < 3; distance++) {
            foreach (var c in key.center.adjecency(distance, chunk_size)) {
                if (chunks.ContainsKey((c.X, c.Y, key.resolution))) continue;

                var a_chunk = new TerrainChunk(this, (c.X, c.Y), chunk_size, key.resolution);
                chunks.Add(a_chunk.key, a_chunk);

                BackgroundTaskScheduler.schedule(
                    a_chunk.key.ToString(),
                    a_chunk.create,
                    () => {
                        a_chunk.upload();
                        renderer.vertex_arrays.Add(a_chunk.vertex_array!);
                    },
                    priority: distance
                );
            }
        }
    }
}

public sealed class TerrainShapeGenerator : IShapeGenerator {
    public struct Options;

    private readonly TerrainChunk chunk;
    private readonly Options options;

    private readonly Plane plane;

    private readonly int size;
    private readonly int pixel_count;

    private readonly float offset_x;
    private readonly float offset_y;

    internal TerrainShapeGenerator(TerrainChunk chunk, Options options = new()) {
        this.chunk = chunk;
        this.options = options;

        plane = chunk.terrain.plane;
        size = chunk.size;
        pixel_count = chunk.size * chunk.resolution;

        offset_x = chunk.center.X - size * 0.5f;
        offset_y = chunk.center.Y - size * 0.5f;
    }

    public IEnumerable<Vector3> get_vertices() {
        /*var n1 = new FastNoiseLite(seed: 9876);
        n1.SetFractalType(FastNoiseLite.FractalType.FBm);
        n1.SetFractalOctaves(6);
        n1.SetFrequency(0.01f);
        n1.SetFractalLacunarity(2f);
        n1.SetFractalGain(0.35f);
        n1.SetFractalWeightedStrength(3.5f);*/

        var n = chunk.terrain.noise.sample(
            pixel_count + 1, pixel_count + 1,
            offset_x, offset_y,
            (float)size / chunk.resolution, (float)size / chunk.resolution
        );

        for (int x = 0; x < chunk.resolution + 1; x++) {
            for (int y = 0; y < chunk.resolution + 1; y++) {
                yield return plane.to_world(
                    (float)x * size / chunk.resolution + offset_x,
                    (float)y * size / chunk.resolution + offset_y, n[x, y]
                );
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
        for (int x = 1; x < chunk.resolution + 1; ++x) {
            for (int y = 1; y < chunk.resolution + 1; ++y) {
                var bottom_left = (y - 1) + (x - 1) * (chunk.resolution + 1);
                var bottom_right = (y - 1) + x * (chunk.resolution + 1);
                var top_left = y + (x - 1) * (chunk.resolution + 1);
                var top_right = y + x * (chunk.resolution + 1);

                yield return (bottom_left, bottom_right, top_left);
                yield return (top_left, bottom_right, top_right);
            }
        }
    }
}