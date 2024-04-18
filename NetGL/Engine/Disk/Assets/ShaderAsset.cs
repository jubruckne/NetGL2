namespace NetGL;

public class ShaderAsset: Asset<ShaderAsset, Shader>, IAssetType<Shader> {
    public static string path => "Shaders";

    public static void save_to_file(Shader asset, string filename) {
        throw new NotImplementedException();
    }

    public static Shader load_from_file(string filename) {
        throw new NotImplementedException();
    }

    public ShaderAsset(string name, Shader data): base(name, data) {}
}