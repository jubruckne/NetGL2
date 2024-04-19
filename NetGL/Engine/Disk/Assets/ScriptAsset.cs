namespace NetGL;

public class ScriptAsset: Asset<ScriptAsset, Script>, IAsset<Script> {
    public static string path => "Scripts";

    public static Script load_from_file(string filename) {
        return new Script(File.ReadAllText(filename));
    }

    private ScriptAsset(string name, Script data): base(name, data) {}

    public static void serialize(Script asset, AssetWriter writer) {
        throw new NotImplementedException();
    }

    public static Script deserialize(AssetReader reader) {
        throw new NotImplementedException();
    }
}