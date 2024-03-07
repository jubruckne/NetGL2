using Assimp;
using Assimp.Configs;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class Model {
    public static string base_path = $"{AppDomain.CurrentDomain.BaseDirectory}../../Assets/Models/";

    public readonly string name;
    public readonly IReadOnlyList<VertexArray> vertex_arrays;
    public readonly IReadOnlyList<Material> materials;
    public readonly IReadOnlyList<Texture> textures;

    private Model(string name) {
        this.name = name;
        vertex_arrays = new List<VertexArray>();
        materials = new List<Material>();
        textures = new List<Texture>();
    }

    private void add_vertex_array(in VertexArray va) => ((List<VertexArray>)vertex_arrays).Add(va);
    private void add_material(in Material mat) => ((List<Material>)materials).Add(mat);

    public static Model from_shape(IShapeGenerator shape_generator) {
        VertexBuffer<Struct<Vector3, Vector3>> vb = new(shape_generator.get_vertices_and_normals(), VertexAttribute.Position, VertexAttribute.Normal);
        var ib = IndexBuffer.create(shape_generator.get_indices(), vb.count);
        vb.upload();
        ib.upload();

        var va = new VertexArrayIndexed(ib, vb);

        va.upload();

        var model = new Model(shape_generator.ToString() ?? "Shape");
        model.add_vertex_array(va);

        return model;
    }

    public static Model from_file(string filename, float scale = 1.0f) {
        if (!File.Exists(filename))
            filename = base_path + filename;

        if (!File.Exists(filename))
            throw new ArgumentOutOfRangeException($"{nameof(filename)}:{filename}");

        var importer = new AssimpContext();
        importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
        //importer.SetConfig(new MeshVertexLimitConfig(165000));

        importer.Scale = scale;
        var assimp = importer.ImportFile(filename, PostProcessSteps.EmbedTextures | /* PostProcessSteps.SplitLargeMeshes | */ PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices | PostProcessSteps.GlobalScale);
        foreach (var tex in assimp.Textures) {
            Console.WriteLine(tex.Filename);
            Console.WriteLine(tex.Width + ":" + tex.Height);
        }

        var result = new Model(Path.GetFileName(filename));

        Console.WriteLine("Materials:");
        foreach (var mat in assimp.Materials) {
            Console.WriteLine($"{mat.Name}: shininess:{mat.Shininess}, strength: {mat.ShininessStrength}");
            result.add_material(new(
                mat.Name,
                ambient_color: (mat.ColorAmbient.R, mat.ColorAmbient.G, mat.ColorAmbient.B),
                specular_color: (mat.ColorSpecular.R, mat.ColorSpecular.G, mat.ColorSpecular.B),
                diffuse_color: (mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B),
                shininess: mat.Shininess / 1000f + 0.0001f));

            if (mat.HasTextureDiffuse) {
                Console.WriteLine(mat.TextureDiffuse.TextureIndex);
            }
        }

        foreach (var mesh in assimp.Meshes) {
            var vb_pos = new VertexBuffer<Assimp.Vector3D>(mesh.Vertices, VertexAttribute.Position);
            vb_pos.upload();
            var vb_norm = new VertexBuffer<Assimp.Vector3D>(mesh.Normals, VertexAttribute.Normal);
            vb_norm.upload();

            var ib = IndexBuffer<int>.make(mesh.GetIndices());
            ib.upload();

            var va = new VertexArrayIndexed(ib, vb_pos, vb_norm);
            va.upload();

            result.add_vertex_array(va);
        }

        return result;
    }

    public override string ToString() {
        return $"Model[name:{name}]";
    }
}