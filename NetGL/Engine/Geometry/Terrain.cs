using NetGL.ECS;
using OpenTK.Mathematics;

namespace NetGL;

internal class TerrainChunk: IShape<TerrainChunk> {
    public readonly Terrain terrain;
    public readonly Vector2i center;
    public readonly Vector2i size;
    public int resolution;
    public VertexArrayIndexed vertex_array;

    private static readonly Dictionary<int, IIndexBuffer> index_buffer_per_resolution = new ();

    public TerrainChunk(Terrain terrain, in Vector2i center, in Vector2i size, int resolution) {
        this.terrain = terrain;
        this.center = center;
        this.size = size;
        this.resolution = resolution;

        vertex_array = create();
    }

    private VertexArrayIndexed create() {
        var gen = generate();
        VertexBuffer<Struct<Vector3, Vector3>> vb = new(gen.get_vertices_and_normals(), VertexAttribute.Position, VertexAttribute.Normal);
        vb.upload();

        if (!index_buffer_per_resolution.TryGetValue(resolution, out var ib)) {
            ib = IndexBuffer.create(gen.get_indices(), vb.count);
            ib.upload();
            index_buffer_per_resolution.Add(resolution, ib);
        }

        var va = new VertexArrayIndexed(ib, vb);
        va.upload();

        return va;
    }

    public void update() => vertex_array = create();

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

    public const int max_resolution = 64;
    public readonly int chunk_size = 256;

    private readonly Dictionary<Vector2i, TerrainChunk> chunks;
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

        chunks = new Dictionary<Vector2i, TerrainChunk>();

        material = this.add_material(Material.random).material;
        renderer = this.add_vertex_array_renderer();
        generate_chunk((0, 0), max_resolution);
        shader = this.add_shader(AutoShader.for_vertex_type($"{name}.auto", chunks[(0, 0)].vertex_array, material));
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

        if (!chunks.ContainsKey((x, y)))
            generate_chunk((x, y), max_resolution);
        else {
            if (chunks[(x, y)].resolution != max_resolution) {
                renderer.vertex_arrays.Remove(chunks[(x, y)].vertex_array);
                chunks[(x, y)].resolution = max_resolution;
                chunks[(x, y)].update();
                renderer.vertex_arrays.Add(chunks[(x, y)].vertex_array);
            }
        }
    }

    private void generate_chunk(in Vector2i center, int resolution = max_resolution) {
        Console.WriteLine($"generating chunk {center}, resolution = {resolution}...");
        var chunk = new TerrainChunk(this, center, (chunk_size, chunk_size), resolution);
        chunks.Add(center, chunk);
        renderer.vertex_arrays.Add(chunk.vertex_array);

        Console.WriteLine($"fw = {camera.transform.rotation.forward}");
        var forward_chunk = camera.transform.position + camera.transform.rotation.forward * chunk_size;
        var (fw_position, height) = plane.world_to_point_on_plane(forward_chunk);
        Console.WriteLine($"fw_pos = {fw_position}");


        int x = (int)(Math.Round(fw_position.X / chunk_size) * chunk_size);
        int y = (int)(Math.Round(fw_position.Y / chunk_size) * chunk_size);

        Console.WriteLine($"generating chunk {(x, y)}, resolution = {resolution}...");
        chunk = new TerrainChunk(this, (x,y), (chunk_size, chunk_size), (int)(resolution * 0.5f));
        chunks.Add((x, y), chunk);
        renderer.vertex_arrays.Add(chunk.vertex_array);
        /*
        if (resolution > 128) {
            (int x, int y)[] directions = [
                (-1, 0), (-1, 1), (0, 1),
                (1, 1),                 (1, 0),
                (1, -1), (0, -1), (-1, -1)
            ];

            foreach (var d in directions) {
                Vector2i k_half = (center.X + d.x * chunk_size, center.Y + d.y * chunk_size);

                if (!chunks.ContainsKey(k_half))
                    generate_chunk(k_half, (int)(resolution * 0.5));
            }
        }
        */
    }
}

public sealed class TerrainShapeGenerator: IShapeGenerator {
    public struct Options;

    private readonly TerrainChunk chunk;
    private readonly Options options;

    private readonly Plane plane;

    private readonly int width;
    private readonly int height;

    private readonly float offset_x;
    private readonly float offset_y;

    internal TerrainShapeGenerator(TerrainChunk chunk, Options options = new()) {
        this.chunk = chunk;
        this.options = options;

        plane = chunk.terrain.plane;

        width = chunk.size.X;
        height = chunk.size.Y;

        offset_x = chunk.center.X - width * 0.5f;
        offset_y = chunk.center.Y - height * 0.5f;
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
            chunk.resolution + 1, chunk.resolution + 1,
            offset_x, offset_y,
            (float)width / chunk.resolution, (float)height / chunk.resolution
        );

        for (int x = 0; x < chunk.resolution + 1; x++) {
            for (int y = 0; y < chunk.resolution + 1; y++) {
                yield return plane.to_world(
                    (float)x * width / chunk.resolution + offset_x,
                    (float)y * height / chunk.resolution + offset_y, n[x, y]
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