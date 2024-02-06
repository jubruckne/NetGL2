using OpenTK.Mathematics;

namespace NetGL;

public class Material {
    public readonly string name;
    public Color4 ambient;
    public Color4 diffuse;
    public Color4 specular;
    public float shininess;

    public Material(in string name, in Color4 ambient, in Color4 diffuse, in Color4 specular, in float shininess) {
        this.name = name;
        this.ambient = ambient;
        this.diffuse = diffuse;
        this.specular = specular;
        this.shininess = shininess;
    }

    public Material(
        in string name,
        in (float r, float g, float b) ambient,
        in (float r, float g, float b) specular,
        in (float r, float g, float b) diffuse,
        in float shininess) {
        this.name = name;
        this.ambient = new Color4(ambient.r, ambient.g, ambient.b, 1f);
        this.specular = new Color4(specular.r, specular.g, specular.b, 1f);
        this.diffuse = new Color4(diffuse.r, diffuse.g, diffuse.b, 1f);
        this.shininess = shininess;
    }

    public override string ToString() {
        if (color_to_name.TryGetValue(this, out var name))
            return name;

        return
            $"ambient:({ambient.R},{ambient.G},{ambient.B}) diffuse:({diffuse.R}, {diffuse.G}, {diffuse.B}), specular:({specular.R}, {specular.G}, {specular.B}), shininess:{shininess}";
    }

    public bool Equals(Material other) {
        return ambient.Equals(other.ambient)
               && diffuse.Equals(other.diffuse)
               && specular.Equals(other.specular)
               && shininess.Equals(other.shininess);
    }

    public override bool Equals(object? obj) {
        return obj is Material other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(ambient, diffuse, specular, shininess);
    }

    public static Material SolidBlue => new("SolidBlue",
        ambient: (0f, 0f, 1f),
        specular: (0.07568f, 0.61424f, 0.07568f),
        diffuse: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f
    );

    public static Material Emerald => new("Emerald",
        ambient: (0.0215f, 0.1745f, 0.0215f),
        specular: (0.07568f, 0.61424f, 0.07568f),
        diffuse: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f
    );

    public static Material Jade => new("Jade",
        ambient: (0.135f, 0.2225f, 0.1575f),
        diffuse: (0.54f, 0.89f, 0.63f),
        specular: (0.316228f, 0.316228f, 0.316228f),
        shininess: 0.1f
    );

    public static Material Obsidian => new("Obsidian",
        ambient: (0.05375f, 0.05f, 0.06625f),
        diffuse: (0.18275f, 0.17f, 0.22525f),
        specular: (0.332741f, 0.328634f, 0.346435f),
        shininess: 0.3f
    );

    public static Material Pearl => new("Pearl",
        ambient: (0.25f, 0.20725f, 0.20725f),
        diffuse: (1f, 0.829f, 0.829f),
        specular: (0.296648f, 0.296648f, 0.296648f),
        shininess: 0.088f
    );

    public static Material Ruby => new("Ruby",
        ambient: (0.1745f, 0.01175f, 0.01175f),
        diffuse: (0.61424f, 0.04136f, 0.04136f),
        specular: (0.727811f, 0.626959f, 0.626959f),
        shininess: 0.6f
    );

    public static Material Turquoise => new("Turquoise",
        ambient: (0.1f, 0.18725f, 0.1745f),
        diffuse: (0.396f, 0.74151f, 0.69102f),
        specular: (0.297254f, 0.30829f, 0.306678f),
        shininess: 0.1f
    );

    public static Material Brass => new("Brass",
        ambient: (0.329412f, 0.223529f, 0.027451f),
        diffuse: (0.780392f, 0.568627f, 0.113725f),
        specular: (0.992157f, 0.941176f, 0.807843f),
        shininess: 0.21794872f
    );

    public static Material Bronze => new("Bronze",
        ambient: (0.2125f, 0.1275f, 0.054f),
        diffuse: (0.714f, 0.4284f, 0.18144f),
        specular: (0.393548f, 0.271906f, 0.166721f),
        shininess: 0.2f
    );

    public static Material Chrome => new("Chrome",
        ambient: (0.25f, 0.25f, 0.25f),
        diffuse: (0.4f, 0.4f, 0.4f),
        specular: (0.774597f, 0.774597f, 0.774597f),
        shininess: 0.6f
    );

    public static Material Copper => new("Copper",
        ambient: (0.19125f, 0.0735f, 0.0225f),
        diffuse: (0.7038f, 0.27048f, 0.0828f),
        specular: (0.256777f, 0.137622f, 0.086014f),
        shininess: 0.1f
    );

    public static Material Gold => new("Gold",
        ambient: (0.24725f, 0.1995f, 0.0745f),
        diffuse: (0.75164f, 0.60648f, 0.22648f),
        specular: (0.628281f, 0.555802f, 0.366065f),
        shininess: 0.4f
    );

    public static Material Silver => new("Silver",
        ambient: (0.19225f, 0.19225f, 0.19225f),
        diffuse: (0.50754f, 0.50754f, 0.50754f),
        specular: (0.508273f, 0.508273f, 0.508273f),
        shininess: 0.4f
    );

    public static Material BlackPlastic => new("Black Plastic",
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.01f, 0.01f, 0.01f),
        specular: (0.50f, 0.50f, 0.50f),
        shininess: 0.25f
    );

    public static Material CyanPlastic => new("Cyan Plastic",
        ambient: (0.0f, 0.1f, 0.06f),
        diffuse: (0.0f, 0.50980392f, 0.50980392f),
        specular: (0.50196078f, 0.50196078f, 0.50196078f),
        shininess: 0.25f
    );

    public static Material GreenPlastic => new("Green Plastic",
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.1f, 0.35f, 0.1f),
        specular: (0.45f, 0.55f, 0.45f),
        shininess: 0.25f
    );

    public static Material RedPlastic => new("Red Plastic",
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.0f, 0.0f),
        specular: (0.7f, 0.6f, 0.6f),
        shininess: 0.25f
    );

    public static Material WhitePlastic => new("White Plastic",
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.55f, 0.55f, 0.55f),
        specular: (0.70f, 0.70f, 0.70f),
        shininess: 0.25f
    );

    public static Material YellowPlastic => new("Yellow Plastic",
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.5f, 0.0f),
        specular: (0.60f, 0.60f, 0.50f),
        shininess: 0.25f
    );

    public static Material BlackRubber => new("Black Rubber",
        ambient: (0.02f, 0.02f, 0.02f),
        diffuse: (0.01f, 0.01f, 0.01f),
        specular: (0.4f, 0.4f, 0.4f),
        shininess: 0.078125f
    );

    public static Material CyanRubber => new("Cyan Rubber",
        ambient: (0.0f, 0.05f, 0.05f),
        diffuse: (0.4f, 0.5f, 0.5f),
        specular: (0.04f, 0.7f, 0.7f),
        shininess: 0.078125f
    );

    public static Material GreenRubber => new("Green Rubber",
        ambient: (0.0f, 0.05f, 0.0f),
        diffuse: (0.4f, 0.5f, 0.4f),
        specular: (0.04f, 0.7f, 0.04f),
        shininess: 0.078125f
    );

    public static Material RedRubber => new("Red Rubber",
        ambient: (0.05f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.4f, 0.4f),
        specular: (0.7f, 0.04f, 0.04f),
        shininess: 0.078125f
    );

    public static Material WhiteRubber => new("White Rubber",
        ambient: (0.05f, 0.05f, 0.05f),
        diffuse: (0.5f, 0.5f, 0.5f),
        specular: (0.7f, 0.7f, 0.7f),
        shininess: 0.078125f
    );

    public static Material YellowRubber => new("Yellow Rubber",
        ambient: (0.05f, 0.05f, 0.0f),
        diffuse: (0.5f, 0.5f, 0.4f),
        specular: (0.7f, 0.7f, 0.04f),
        shininess: 0.078125f
    );

    private static readonly IReadOnlyDictionary<Material, string> color_to_name = new Dictionary<Material, string> {
        { SolidBlue, "Solid Blue" },
        { Emerald, "Emerald" },
        { Jade, "Jade" },
        { Obsidian, "Obsidian" },
        { Pearl, "Pearl" },
        { Ruby, "Ruby" },
        { Turquoise, "Turquoise" },
        { Brass, "Brass" },
        { Bronze, "Bronze" },
        { Chrome, "Chrome" },
        { Copper, "Copper" },
        { Gold, "Gold" },
        { Silver, "Silver" },
        { BlackPlastic, "Black Plastic" },
        { CyanPlastic, "Cyan Plastic" },
        { GreenPlastic, "Green Plastic" },
        { RedPlastic, "Red Plastic" },
        { WhitePlastic, "White Plastic" },
        { YellowPlastic, "Yellow Plastic" },
        { BlackRubber, "Black Rubber" },
        { CyanRubber, "Cyan Rubber" },
        { GreenRubber, "Green Rubber" },
        { RedRubber, "Red Rubber" },
        { WhiteRubber, "White Rubber" },
        { YellowRubber, "Yellow Rubber" }
    };
}