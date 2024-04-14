namespace NetGL;

using StbImageSharp;

public class Image: IAssetType<Image> {
    public readonly int width;
    public readonly int height;
    public readonly byte[] image_data;

    public static string path => "Textures";

    public static Image load_from_file(string path) {
        int width;
        int height;
        byte[] image_data;

        StbImage.stbi_set_flip_vertically_on_load(1);

        if (!File.Exists(path)) {
            Console.WriteLine($"file not found: {path}!");
            throw new FileNotFoundException(path);
        }

        using (Stream stream = File.OpenRead(path)) {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            image_data = image.Data;
            width = image.Width;
            height = image.Height;
        }

        return new Image(width, height, image_data);
    }

    public Image(int width, int height, in byte[] image_data) {
        this.width = width;
        this.height = height;
        this.image_data = image_data;
    }
}