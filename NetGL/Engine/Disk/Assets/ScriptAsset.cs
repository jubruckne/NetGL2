namespace NetGL;

public class ScriptAsset: Asset<ScriptAsset, Script>, IAssetType<Script> {
    public static string path => "Scripts";

    public static Script load_from_file(string filename) {
        return new Script(File.ReadAllText(filename));
    }

    public static void save_to_file(Script asset, string filename) {
        throw new NotImplementedException();
    }

    public ScriptAsset(string name, Script data): base(name, data) {}
}