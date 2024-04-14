namespace NetGL.Materials;

using System.Text;
using ECS;

public partial class Material: INamed {
    public string name { get; }
    public Shader shader { get; }
    public NamedBag<Property> inputs { get; }
    public NamedBag<Property> properties { get; }

    public Material(in string name, in Shader shader) {
        this.name = name;
        this.shader = shader;
        this.properties = [];
        this.inputs = [];
    }

    public Material(in string name, in Material from) {
        this.name = name;
        shader = from.shader;
        properties = new(from.properties);
        inputs = new(from.inputs);
    }

    public void add_texture<T>(string name, T texture) where T: Texture {
        var item = new Property<T>(name, texture);
        inputs.add(item);
    }

    public void add_color(string name, Color color) {
        var item = new Property<Color>(name, color);
        properties.add(item);
    }

    public string get_declaration() {
        var sb = new StringBuilder();

        foreach (var property in inputs) {
            var glsl_type = property switch {
                Property<Texture2D<float4>> tex => tex.value!.sampler_name,
                Property<Texture2D<uint>> tex   => tex.value.sampler_name,
                Property<Texture2D<float>> tex  => tex.value.sampler_name,
                Property<Texture2D<byte>> tex   => tex.value.sampler_name,
                Property<Texture2D<half>> tex   => tex.value.sampler_name,
                _                              => throw new NotImplementedException()
            };

            sb.AppendLine($"uniform {glsl_type} {name.ToLower()}_{property.name};");
        }

        sb.AppendLine();

        sb.AppendLine($"struct {name.ToLower()}_t {{");
        foreach (var property in properties) {
            var glsl_type = property switch {
                Property<float>                 => "float",
                Property<int>                   => "int",
                Property<Color>                 => "vec4",
                _                               => throw new NotImplementedException()
            };

            sb.AppendLine($"    {glsl_type} {property.name};");
        }
        sb.AppendLine("}");

        sb.AppendLine($"uniform {name.ToLower()}_t {name.ToLower()};");


        return sb.ToString();
    }

    public override string ToString()
        => name;
}

public partial class Material {
    public class Default: Material {
        public static Material.Default Blue => new(
                                                   "Blue",
                                                   null,
                                                   ambient_color: (0f, 0f, 1f),
                                                   specular_color: (0.1f, 0.1f, 0.5f),
                                                   diffuse_color: (0.1f, 0.1f, 0.5f),
                                                   shininess: 100f
                                                  );

        public float shininess {
            get => (Property<float>)properties["shininess"];
            set => ((Property<float>)properties["shininess"]).value = value;
        }

        public Color ambient_color {
            get => (Property<Color>)properties["ambient_color"];
            set => ((Property<Color>)properties["ambient_color"]).value = value;
        }

        public Color diffuse_color {
            get => (Property<Color>)properties["diffuse_color"];
            set => ((Property<Color>)properties["diffuse_color"]).value = value;
        }

        public Color specular_color {
            get => (Property<Color>)properties["specular_color"];
            set => ((Property<Color>)properties["specular_color"]).value = value;
        }

        public Texture? ambient_texture {
            get => ((Property<Texture>)properties["ambient_texture"]).value ?? null;
            set => ((Property<Texture>)properties["ambient_texture"]).value = value;
        }

        public Default(in string name, in Shader shader): base(in name, in shader) {
            properties.add(new Property<float>("shininess", 32.0f));
            properties.add(new Property<Color>("ambient_color", Color.White));
            properties.add(new Property<Color>("diffuse_color", Color.White));
            properties.add(new Property<Color>("specular_color", Color.White));
            properties.add(new Property<Image>("ambient_texture"));
        }

        public Default(in string name,
                       in Shader shader,
                       in Color ambient_color,
                       in Color diffuse_color,
                       in Color specular_color,
                       in float shininess
        ): base(in name, in shader) {
            properties.add(new Property<float>("shininess", shininess));
            properties.add(new Property<Color>("ambient_color", ambient_color));
            properties.add(new Property<Color>("diffuse_color", diffuse_color));
            properties.add(new Property<Color>("specular_color", specular_color));
            properties.add(new Property<Image>("ambient_texture"));
        }
    }
}