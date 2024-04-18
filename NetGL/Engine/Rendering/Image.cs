namespace NetGL;

public class Image {
    public readonly int width;
    public readonly int height;
    public readonly byte[] image_data;

    public Image(int width, int height, in byte[] image_data) {
        this.width = width;
        this.height = height;
        this.image_data = image_data;
    }
}