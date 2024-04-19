namespace NetGL;

public class ShaderAsset: Asset<ShaderAsset, Shader>, IAsset<Shader> {
    public static string path => "Shaders";

    private ShaderAsset(string name, Shader data): base(name, data) {}

    public static void serialize(Shader asset, AssetWriter writer) {
        throw new NotImplementedException();
    }

    public static Shader deserialize(AssetWriter reader) {
        throw new NotImplementedException();
    }
}