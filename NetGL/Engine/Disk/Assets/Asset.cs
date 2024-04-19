using NetGL.ECS;

namespace NetGL;

public interface IAsset {
    static abstract string path { get; }
}

public interface IAsset<T>: IAsset {
    static abstract void serialize(T asset, AssetWriter writer);
    static abstract T deserialize(AssetWriter reader);
}

public static class Asset {
    public static readonly string asset_root = "";

    static Asset() {
        var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        while (current.Parent != null) {
            // Move to the parent directory
            current = current.Parent;

            // Check if a subdirectory named "Assets" exists
            var target_dir = current.GetDirectories("Assets").FirstOrDefault();
            if (target_dir == null) continue;
            Console.WriteLine($"Assets library found in {target_dir.FullName}.");
            asset_root = target_dir.FullName;
            break;
        }

        if (asset_root == "")
            throw new DirectoryNotFoundException(
                                                 $"Assets directory not found ({AppDomain.CurrentDomain.BaseDirectory})!"
                                                );
    }
}

public class Asset<TAsset, T> where TAsset: IAsset<T> {
    public static string get_temp_file_name(string filename) => $"{Asset.asset_root}/Temp/{filename}";

    public readonly string name;
    public readonly T data;

    protected Asset(string name, T data) {
        this.name = name;
        this.data = data;
    }

    public static string resolve_file_name() => $"{Asset.asset_root}/{TAsset.path}/";
    public static string resolve_file_name(string filename) => $"{Asset.asset_root}/{TAsset.path}/{filename}";

    public static IReadOnlyList<string> get_files()
        => Directory.GetFiles(resolve_file_name());

    public static IReadOnlyList<string> get_files(string directory)
        => Directory.GetFiles(resolve_file_name(directory));

    public static void save_to_file(T asset, string filename) {
        using var writer = AssetWriter.open(resolve_file_name(filename));
        TAsset.serialize(asset, writer);
        writer.close();
    }
}

/*
public static class AssetManager {
    public static readonly string asset_root = "";

    static AssetManager() {
        var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        while (current.Parent != null) {
            // Move to the parent directory
            current = current.Parent;

            // Check if a subdirectory named "Assets" exists
            var target_dir = current.GetDirectories("Assets").FirstOrDefault();
            if (target_dir == null) continue;
            Console.WriteLine($"Assets library found in {target_dir.FullName}.");
            asset_root = target_dir.FullName;
            break;
        }

        if(asset_root == "")
            throw new DirectoryNotFoundException($"Assets directory not found ({AppDomain.CurrentDomain.BaseDirectory})!");
    }

    private static readonly Dictionary<int, Asset> library = new();

    public static IReadOnlyList<string> get_files<T>()
        where T : IAssetType<T>
        => Directory.GetFiles(asset_path<T>());

    public static IReadOnlyList<string> get_files<T>(string directory)
        where T : IAssetType<T>
        => Directory.GetFiles(asset_path<T>(directory));


    public static string asset_path<T>() where T : IAssetType<T> => $"{asset_root}/{T.path}/";
    public static string asset_path<T>(string filename) where T : IAssetType => $"{asset_root}/{T.path}/{filename}";

    public static void add<T>(string name, in T data) where T: IAssetType<T> {
        var key = typeof(T).GetHashCode() ^ name.GetHashCode();
        library.Add(key, new Asset<T>(name, data));
    }

    public static ref readonly T get<T>(string name) where T: IAssetType<T> {
        var key = typeof(T).GetHashCode() ^ name.GetHashCode();
        var asset = library[key];
        return ref ((Asset<T>)asset).data;
    }

    public static void for_each<T>(Action<T> action) where T: IAssetType<T> {
        foreach (var asset in library.Values)
            if (asset is Asset<T> at) action(at.data);
    }

    public static IEnumerable<T> get_all<T>() where T: IAssetType<T> {
        foreach (var asset in library.Values)
            if (asset is Asset<T> at) yield return at.data;
    }

    public static void load_all_files<T>() where T: IAssetType<T> {
        foreach (var file in get_files<T>())
            load_from_file<T>(file);
    }

    public static ref readonly T load_from_file<T>(string filename) where T: IAssetType<T> {
        // Console.WriteLine($"Loading {filename}...");

        if(!Path.Exists(filename))
            filename = $"{asset_root}/{T.path}/{filename}";

        var data = T.load_from_file(filename);
        add(filename, data);
        return ref get<T>(filename);
    }
}
*/