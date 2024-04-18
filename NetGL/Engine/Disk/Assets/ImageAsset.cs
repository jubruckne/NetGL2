using StbImageSharp;

namespace NetGL;

public class ImageAsset: Asset<ImageAsset, Image>, IAssetType<Image> {
    public static string path => "Textures";

    public ImageAsset(string name, in Image data): base(name, data) {}

    public static Image load_from_file(string filename) {
        int    width;
        int    height;
        byte[] image_data;

        filename = resolve_file_name(filename);

        StbImage.stbi_set_flip_vertically_on_load(1);

        if (!File.Exists(filename)) {
            Console.WriteLine($"file not found: {filename}!");
            throw new FileNotFoundException(filename);
        }

        using (Stream stream = File.OpenRead(filename)) {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            image_data = image.Data;
            width      = image.Width;
            height     = image.Height;
        }

        return new Image(width, height, image_data);
    }

    public static void save_to_file(Image asset, string filename) {
        throw new NotImplementedException();
    }
}