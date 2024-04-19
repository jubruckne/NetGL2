namespace NetGL;

public class HeightmapAsset: Asset<HeightmapAsset, Heightmap>, IAsset<Heightmap> {
    public static string path => "Heightmaps";

    private HeightmapAsset(string name, Heightmap data): base(name, data) {}

    public static void serialize(Heightmap asset, AssetWriter writer) {
        writer.write("name", asset.name);
        writer.write("bounds", asset.bounds);
        writer.write("texture_size", asset.texture_size);
        writer.write("data", asset.texture.as_readonly_span());
    }

    public static Heightmap deserialize(AssetReader reader) {
        return new Heightmap(0, new Rectangle());
    }
}