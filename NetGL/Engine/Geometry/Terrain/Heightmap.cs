namespace NetGL;

public class Heightmap {
    public readonly int size;
    public readonly int resolution;
    public readonly Field<float> heights;

    public Heightmap(int size, int resolution) {
        this.size = size;
        this.resolution = resolution;
        this.heights = new Field<float>(resolution, resolution);
    }

    public void generate() {

    }

}