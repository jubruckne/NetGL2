using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL;

public class Material {
    [StructLayout(LayoutKind.Sequential)]
    public struct Color: IEquatable<Color> {
        public Color4 ambient;
        public Color4 diffuse;
        public Color4 specular;
        public float shininess;

        public Color(in Color4 ambient, in Color4 diffuse, in Color4 specular, in float shininess) {
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
            this.shininess = shininess;
        }

        public Color((float r, float g, float b) ambient, (float r, float g, float b) specular, (float r, float g, float b) diffuse, float shininess) {
            this.ambient = new Color4(ambient.r, ambient.g, ambient.b, 1f);
            this.specular = new Color4(specular.r, specular.g, specular.b, 1f);
            this.diffuse = new Color4(diffuse.r, diffuse.g, diffuse.b, 1f);
            this.shininess = shininess;
        }

        public override string ToString() {
            if (color_to_name.TryGetValue(this, out var name))
                return name;

            return $"ambient:{ambient}, diffuse:{diffuse}, specular:{specular}, shininess{shininess}";
        }

        public bool Equals(Color other) {
            return ambient.Equals(other.ambient)
                   && diffuse.Equals(other.diffuse)
                   && specular.Equals(other.specular)
                   && shininess.Equals(other.shininess);
        }

        public override bool Equals(object? obj) {
            return obj is Color other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ambient, diffuse, specular, shininess);
        }
    }

    public static Color SolidBlue => new(
        ambient: (0f, 0f, 1f),
        specular: (0.07568f, 0.61424f, 0.07568f),
        diffuse: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f
    );

    public static Color Emerald => new(
        ambient: (0.0215f, 0.1745f, 0.0215f),
        specular: (0.07568f, 0.61424f, 0.07568f),
        diffuse: (0.633f, 0.727811f, 0.633f),
        shininess: 0.6f
    );

    public static Color Jade => new(
        ambient: (0.135f, 0.2225f, 0.1575f),
        diffuse: (0.54f, 0.89f, 0.63f),
        specular: (0.316228f, 0.316228f, 0.316228f),
        shininess: 0.1f
    );

    public static Color Obsidian => new(
        ambient: (0.05375f, 0.05f, 0.06625f),
        diffuse: (0.18275f, 0.17f, 0.22525f),
        specular: (0.332741f, 0.328634f, 0.346435f),
        shininess: 0.3f
    );

    public static Color Pearl => new(
        ambient: (0.25f, 0.20725f, 0.20725f),
        diffuse: (1f, 0.829f, 0.829f),
        specular: (0.296648f, 0.296648f, 0.296648f),
        shininess: 0.088f
    );

    public static Color Ruby => new(
        ambient: (0.1745f, 0.01175f, 0.01175f),
        diffuse: (0.61424f, 0.04136f, 0.04136f),
        specular: (0.727811f, 0.626959f, 0.626959f),
        shininess: 0.6f
    );

    public static Color Turquoise => new(
        ambient: (0.1f, 0.18725f, 0.1745f),
        diffuse: (0.396f, 0.74151f, 0.69102f),
        specular: (0.297254f, 0.30829f, 0.306678f),
        shininess: 0.1f
    );

    public static Color Brass => new(
        ambient: (0.329412f, 0.223529f, 0.027451f),
        diffuse: (0.780392f, 0.568627f, 0.113725f),
        specular: (0.992157f, 0.941176f, 0.807843f),
        shininess: 0.21794872f
    );

    public static Color Bronze => new(
        ambient: (0.2125f, 0.1275f, 0.054f),
        diffuse: (0.714f, 0.4284f, 0.18144f),
        specular: (0.393548f, 0.271906f, 0.166721f),
        shininess: 0.2f
    );

    public static Color Chrome => new(
        ambient: (0.25f, 0.25f, 0.25f),
        diffuse: (0.4f, 0.4f, 0.4f),
        specular: (0.774597f, 0.774597f, 0.774597f),
        shininess: 0.6f
    );

    public static Color Copper => new(
        ambient: (0.19125f, 0.0735f, 0.0225f),
        diffuse: (0.7038f, 0.27048f, 0.0828f),
        specular: (0.256777f, 0.137622f, 0.086014f),
        shininess: 0.1f
    );

    public static Color Gold => new(
        ambient: (0.24725f, 0.1995f, 0.0745f),
        diffuse: (0.75164f, 0.60648f, 0.22648f),
        specular: (0.628281f, 0.555802f, 0.366065f),
        shininess: 0.4f
    );

    public static Color Silver => new(
        ambient: (0.19225f, 0.19225f, 0.19225f),
        diffuse: (0.50754f, 0.50754f, 0.50754f),
        specular: (0.508273f, 0.508273f, 0.508273f),
        shininess: 0.4f
    );

    public static Color BlackPlastic => new(
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.01f, 0.01f, 0.01f),
        specular: (0.50f, 0.50f, 0.50f),
        shininess: 0.25f
    );

    public static Color CyanPlastic => new(
        ambient: (0.0f, 0.1f, 0.06f),
        diffuse: (0.0f, 0.50980392f, 0.50980392f),
        specular: (0.50196078f, 0.50196078f, 0.50196078f),
        shininess: 0.25f
    );

    public static Color GreenPlastic => new(
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.1f, 0.35f, 0.1f),
        specular: (0.45f, 0.55f, 0.45f),
        shininess: 0.25f
    );

    public static Color RedPlastic => new(
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.0f, 0.0f),
        specular: (0.7f, 0.6f, 0.6f),
        shininess: 0.25f
    );

    public static Color WhitePlastic => new(
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.55f, 0.55f, 0.55f),
        specular: (0.70f, 0.70f, 0.70f),
        shininess: 0.25f
    );

    public static Color YellowPlastic => new(
        ambient: (0.0f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.5f, 0.0f),
        specular: (0.60f, 0.60f, 0.50f),
        shininess: 0.25f
    );

    public static Color BlackRubber => new(
        ambient: (0.02f, 0.02f, 0.02f),
        diffuse: (0.01f, 0.01f, 0.01f),
        specular: (0.4f, 0.4f, 0.4f),
        shininess: 0.078125f
    );

    public static Color CyanRubber => new(
        ambient: (0.0f, 0.05f, 0.05f),
        diffuse: (0.4f, 0.5f, 0.5f),
        specular: (0.04f, 0.7f, 0.7f),
        shininess: 0.078125f
    );

    public static Color GreenRubber => new(
        ambient: (0.0f, 0.05f, 0.0f),
        diffuse: (0.4f, 0.5f, 0.4f),
        specular: (0.04f, 0.7f, 0.04f),
        shininess: 0.078125f
    );

    public static Color RedRubber => new(
        ambient: (0.05f, 0.0f, 0.0f),
        diffuse: (0.5f, 0.4f, 0.4f),
        specular: (0.7f, 0.04f, 0.04f),
        shininess: 0.078125f
    );

    public static Color WhiteRubber => new(
        ambient: (0.05f, 0.05f, 0.05f),
        diffuse: (0.5f, 0.5f, 0.5f),
        specular: (0.7f, 0.7f, 0.7f),
        shininess: 0.078125f
    );

    public static Color YellowRubber => new(
        ambient: (0.05f, 0.05f, 0.0f),
        diffuse: (0.5f, 0.5f, 0.4f),
        specular: (0.7f, 0.7f, 0.04f),
        shininess: 0.078125f
    );

    private static readonly IReadOnlyDictionary<Color, string> color_to_name = new Dictionary<Color, string> {
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