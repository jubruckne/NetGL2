namespace NetGL;

public class Material {
    public readonly string name;
    public Color ambient_color;
    public TextureBuffer? ambient_texture;
    public Color diffuse_color;
    public Color specular_color;
    public float shininess;

    public Material(in string name, in Color ambient_color, in Color diffuse_color, in Color specular_color, in float shininess) {
        this.name = name;
        this.ambient_color = ambient_color;
        this.diffuse_color = diffuse_color;
        this.specular_color = specular_color;
        this.shininess = shininess;
    }

    public Material(
        in string name,
        in (float r, float g, float b) ambient_color,
        in (float r, float g, float b) specular_color,
        in (float r, float g, float b) diffuse_color,
        in float shininess) {
        this.name = name;
        this.ambient_color = Color.make(ambient_color);
        this.specular_color = Color.make(specular_color);
        this.diffuse_color = Color.make(diffuse_color);
        this.shininess = shininess;
    }

    public override string ToString() {
        if (color_to_name.TryGetValue(this, out var name))
            return name;

        return
            $"ambient:{ambient_color}, diffuse:{diffuse_color}, specular:{specular_color}, shininess:{shininess}";
    }

    public bool Equals(Material other) {
        return ambient_color.Equals(other.ambient_color)
               && diffuse_color.Equals(other.diffuse_color)
               && specular_color.Equals(other.specular_color)
               && shininess.Equals(other.shininess);
    }

    public override bool Equals(object? obj) {
        return obj is Material other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(ambient_color, diffuse_color, specular_color, shininess);
    }

    public static Material Red => new("Red",
        ambient_color: (1f, 0f, 0f),
        specular_color: (0.1f, 0.1f, 0.5f),
        diffuse_color: (0.1f, 0.1f, 0.5f),
        shininess: 1f
    );

    public static Material Green => new("Green",
        ambient_color: (0f, 1f, 0f),
        specular_color: (0.1f, 0.1f, 0.5f),
        diffuse_color: (0.1f, 0.1f, 0.5f),
        shininess: 1f
    );

    public static Material Blue => new("Blue",
        ambient_color: (0f, 0f, 1f),
        specular_color: (0.1f, 0.1f, 0.5f),
        diffuse_color: (0.1f, 0.1f, 0.5f),
        shininess: 1f
    );



    public static Material SolidBlue => new("SolidBlue",
        ambient_color: (0f, 0f, 1f),
        specular_color: (0.07568f, 0.61424f, 0.07568f),
        diffuse_color: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f * 10f
    );

    public static Material Emerald => new("Emerald",
        ambient_color: (0.0215f, 0.1745f, 0.0215f),
        specular_color: (0.07568f, 0.61424f, 0.07568f),
        diffuse_color: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f * 10f
    );

    public static Material Jade => new("Jade",
        ambient_color: (0.135f, 0.2225f, 0.1575f),
        diffuse_color: (0.54f, 0.89f, 0.63f),
        specular_color: (0.316228f, 0.316228f, 0.316228f),
        shininess: 0.1f * 10f
    );

    public static Material Obsidian => new("Obsidian",
        ambient_color: (0.05375f, 0.05f, 0.06625f),
        diffuse_color: (0.18275f, 0.17f, 0.22525f),
        specular_color: (0.332741f, 0.328634f, 0.346435f),
        shininess: 0.3f * 10f
    );

    public static Material Pearl => new("Pearl",
        ambient_color: (0.25f, 0.20725f, 0.20725f),
        diffuse_color: (1f, 0.829f, 0.829f),
        specular_color: (0.296648f, 0.296648f, 0.296648f),
        shininess: 0.088f * 10f
    );

    public static Material Ruby => new("Ruby",
        ambient_color: (0.1745f, 0.01175f, 0.01175f),
        diffuse_color: (0.61424f, 0.04136f, 0.04136f),
        specular_color: (0.727811f, 0.626959f, 0.626959f),
        shininess: 0.6f * 10f
    );

    public static Material Turquoise => new("Turquoise",
        ambient_color: (0.1f, 0.18725f, 0.1745f),
        diffuse_color: (0.396f, 0.74151f, 0.69102f),
        specular_color: (0.297254f, 0.30829f, 0.306678f),
        shininess: 0.1f * 10f
    );

    public static Material Brass => new("Brass",
        ambient_color: (0.329412f, 0.223529f, 0.027451f),
        diffuse_color: (0.780392f, 0.568627f, 0.113725f),
        specular_color: (0.992157f, 0.941176f, 0.807843f),
        shininess: 0.21794872f * 10f
    );

    public static Material Bronze => new("Bronze",
        ambient_color: (0.2125f, 0.1275f, 0.054f),
        diffuse_color: (0.714f, 0.4284f, 0.18144f),
        specular_color: (0.393548f, 0.271906f, 0.166721f),
        shininess: 0.2f * 10f
    );

    public static Material Chrome => new("Chrome",
        ambient_color: (0.25f, 0.25f, 0.25f),
        diffuse_color: (0.4f, 0.4f, 0.4f),
        specular_color: (0.774597f, 0.774597f, 0.774597f),
        shininess: 0.6f * 10f
    );

    public static Material Copper => new("Copper",
        ambient_color: (0.19125f, 0.0735f, 0.0225f),
        diffuse_color: (0.7038f, 0.27048f, 0.0828f),
        specular_color: (0.256777f, 0.137622f, 0.086014f),
        shininess: 0.1f * 10f
    );

    public static Material Gold => new("Gold",
        ambient_color: (0.24725f, 0.1995f, 0.0745f),
        diffuse_color: (0.75164f, 0.60648f, 0.22648f),
        specular_color: (0.628281f, 0.555802f, 0.366065f),
        shininess: 0.4f * 10f
    );

    public static Material Silver => new("Silver",
        ambient_color: (0.19225f, 0.19225f, 0.19225f),
        diffuse_color: (0.50754f, 0.50754f, 0.50754f),
        specular_color: (0.508273f, 0.508273f, 0.508273f),
        shininess: 0.4f * 10f
    );

    public static Material BlackPlastic => new("Black Plastic",
        ambient_color: (0.0f, 0.0f, 0.0f),
        diffuse_color: (0.01f, 0.01f, 0.01f),
        specular_color: (0.50f, 0.50f, 0.50f),
        shininess: 0.25f * 10f
    );

    public static Material CyanPlastic => new("Cyan Plastic",
        ambient_color: (0.0f, 0.1f, 0.06f),
        diffuse_color: (0.0f, 0.50980392f, 0.50980392f),
        specular_color: (0.50196078f, 0.50196078f, 0.50196078f),
        shininess: 0.25f * 10f
    );

    public static Material GreenPlastic => new("Green Plastic",
        ambient_color: (0.0f, 0.0f, 0.0f),
        diffuse_color: (0.1f, 0.35f, 0.1f),
        specular_color: (0.45f, 0.55f, 0.45f),
        shininess: 0.25f * 10f
    );

    public static Material RedPlastic => new("Red Plastic",
        ambient_color: (0.0f, 0.0f, 0.0f),
        diffuse_color: (0.5f, 0.0f, 0.0f),
        specular_color: (0.7f, 0.6f, 0.6f),
        shininess: 0.25f * 10f
    );

    public static Material WhitePlastic => new("White Plastic",
        ambient_color: (0.0f, 0.0f, 0.0f),
        diffuse_color: (0.55f, 0.55f, 0.55f),
        specular_color: (0.70f, 0.70f, 0.70f),
        shininess: 0.25f * 10f
    );

    public static Material YellowPlastic => new("Yellow Plastic",
        ambient_color: (0.0f, 0.0f, 0.0f),
        diffuse_color: (0.5f, 0.5f, 0.0f),
        specular_color: (0.60f, 0.60f, 0.50f),
        shininess: 0.25f * 10f
    );

    public static Material BlackRubber => new("Black Rubber",
        ambient_color: (0.02f, 0.02f, 0.02f),
        diffuse_color: (0.01f, 0.01f, 0.01f),
        specular_color: (0.4f, 0.4f, 0.4f),
        shininess: 0.078125f * 10f
    );

    public static Material CyanRubber => new("Cyan Rubber",
        ambient_color: (0.0f, 0.05f, 0.05f),
        diffuse_color: (0.4f, 0.5f, 0.5f),
        specular_color: (0.04f, 0.7f, 0.7f),
        shininess: 0.078125f * 10f
    );

    public static Material GreenRubber => new("Green Rubber",
        ambient_color: (0.0f, 0.05f, 0.0f),
        diffuse_color: (0.4f, 0.5f, 0.4f),
        specular_color: (0.04f, 0.7f, 0.04f),
        shininess: 0.078125f * 10f
    );

    public static Material RedRubber => new("Red Rubber",
        ambient_color: (0.05f, 0.0f, 0.0f),
        diffuse_color: (0.5f, 0.4f, 0.4f),
        specular_color: (0.7f, 0.04f, 0.04f),
        shininess: 0.078125f * 10f
    );

    public static Material WhiteRubber => new("White Rubber",
        ambient_color: (0.05f, 0.05f, 0.05f),
        diffuse_color: (0.5f, 0.5f, 0.5f),
        specular_color: (0.7f, 0.7f, 0.7f),
        shininess: 0.078125f * 10f
    );

    public static Material YellowRubber => new("Yellow Rubber",
        ambient_color: (0.05f, 0.05f, 0.0f),
        diffuse_color: (0.5f, 0.5f, 0.4f),
        specular_color: (0.7f, 0.7f, 0.04f),
        shininess: 0.078125f * 10f
    );

    public static Material random => color_to_name.Keys.ToList().random();

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