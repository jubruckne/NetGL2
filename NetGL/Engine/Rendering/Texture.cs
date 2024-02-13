namespace NetGL;

using StbImageSharp;

public class Texture {
    private static readonly string base_path = $"{AppDomain.CurrentDomain.BaseDirectory}../../Assets/Textures/";

    public readonly string name;
    public readonly int width;
    public readonly int height;
    public readonly byte[] image_data;

    public static Texture load_from_file(string path) {
        int width;
        int height;
        byte[] image_data;

        StbImage.stbi_set_flip_vertically_on_load(1);

        if (File.Exists($"{base_path}" + path))
            path = $"{base_path}" + path;

        if(!File.Exists(path))
            throw new FileNotFoundException(path);

        using (Stream stream = File.OpenRead(path)) {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            image_data = image.Data;
            width = image.Width;
            height = image.Height;
        }
     
        return new(Path.GetFileNameWithoutExtension(path), width, height, image_data);
    }

    protected Texture(string name, int width, int height, byte[] image_data) {
        this.width = width;
        this.height = height;
        this.image_data = image_data;
        this.name = name;
    }
}