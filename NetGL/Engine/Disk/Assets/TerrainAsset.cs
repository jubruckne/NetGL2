namespace NetGL;

public class TerrainAsset: Asset<TerrainAsset, Terrain>, IAsset<Terrain> {
    public static string path => "Heightmaps";

    private TerrainAsset(string name, Terrain data): base(name, data) {}

    public static void serialize(Terrain asset, AssetWriter writer) {
        throw new NotImplementedException();
    }

    public static Terrain deserialize(AssetWriter reader) {
        throw new NotImplementedException();
    }
}