namespace NetGL;

public class HeightmapAsset: Asset<HeightmapAsset, Heightmap>, IAsset<Heightmap> {
    public static string path => "Heightmaps";

    private HeightmapAsset(string name, Heightmap data): base(name, data) {}

    public static void serialize(Heightmap asset, AssetWriter writer) {
        writer.write_chunk("bounds", asset.area);
        writer.write_chunk("texture_size", asset.texture_size);
        writer.write_chunk("data", asset.texture.as_readonly_span());
    }

    public static Heightmap deserialize(AssetReader reader) {
        foreach(var chunk in reader.chunks)
            Console.WriteLine($"HeightmapAsset.deserialize: chunk: {chunk}");

        var texture_size = reader.read_chunk<int>("texture_size");
        var area = reader.read_chunk<Rectangle<int>>("bounds");

        var heightmap = new Heightmap(area, Rectangle.with_size(texture_size, texture_size));

        reader.read_chunk("data", heightmap.texture.as_span());

        return heightmap;
    }
}