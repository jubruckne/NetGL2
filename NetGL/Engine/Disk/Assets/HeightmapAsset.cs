namespace NetGL;

public class HeightmapAsset: Asset<HeightmapAsset, Heightmap>, IAssetType<Heightmap> {
    public static string path => "Heightmaps";

    static Heightmap IAssetType<Heightmap>.load_from_file(string filename) {
        throw new NotImplementedException();
    }

    public static void save_to_file(Heightmap heightmap, string filename) {
        using var writer = AssetWriter.open(resolve_file_name(filename));
        writer.write(heightmap.bounds.center.ToString(), heightmap.texture.as_readonly_span());
        writer.close();
    }

    public HeightmapAsset(string name, Heightmap data): base(name, data) {}
}