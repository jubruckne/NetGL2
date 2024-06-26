using StbImageSharp;
using StbImageWriteSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace NetGL;

public class ImageAsset: Asset<ImageAsset, Image>, IAsset<Image> {
    public static string path => "Textures";

    private ImageAsset(string name, in Image data): base(name, data) {}

    public static Image export(Texture2D<float> texture, string filename) {
        StbImageWrite.stbi__flip_vertically_on_write = 1;
        return null;
    }

    public static Image load_from_file(string filename, ColorComponents components = ColorComponents.RedGreenBlueAlpha) {
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
            ImageResult image = ImageResult.FromStream(stream, components);
            image_data = image.Data;
            width      = image.Width;
            height     = image.Height;
        }

        return new Image(width, height, image_data);
    }

    public static void serialize(Image asset, AssetWriter writer) {
        throw new NotImplementedException();
    }

    public static Image deserialize(AssetReader reader) {
        throw new NotImplementedException();
    }
}