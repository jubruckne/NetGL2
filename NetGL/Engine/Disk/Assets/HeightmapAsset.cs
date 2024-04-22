namespace NetGL;

public class HeightmapAsset: Asset<HeightmapAsset, Heightmap>, IAsset<Heightmap> {
    public static string path => "Heightmaps";

    private HeightmapAsset(string name, Heightmap data): base(name, data) {}

    public static void serialize(Heightmap asset, AssetWriter writer) {
        writer.write_chunk("bounds", asset.bounds);
        writer.write_chunk("texture_size", asset.texture_size);
        writer.write_chunk("data", asset.texture.as_readonly_span());
    }

    public static Heightmap deserialize(AssetReader reader) {
        foreach(var chunk in reader.chunks)
            Console.WriteLine($"HeightmapAsset.deserialize: chunk: {chunk}");

        var texture_size = reader.read_chunk<int>("texture_size");
        var bounds = reader.read_chunk<Rectangle>("bounds");

        var heightmap = new Heightmap(texture_size, bounds);

        reader.read_chunk("data", heightmap.texture.as_span());

        return heightmap;
    }
}