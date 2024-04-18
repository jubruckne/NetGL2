namespace NetGL;

public interface IAssetType {
    static abstract string path { get; }
}

public interface IAssetType<T>: IAssetType {
    static abstract T load_from_file(string filename);
    static abstract void save_to_file(T asset, string filename);
}

public abstract class Asset {
    public static readonly string asset_root = "";
    public static string get_temp_file_name(string filename) => $"{asset_root}/Temp/{filename}";

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

        if(asset_root == "")
            throw new DirectoryNotFoundException($"Assets directory not found ({AppDomain.CurrentDomain.BaseDirectory})!");
    }

    public readonly string name;

    protected Asset(string name) {
        this.name = name;
    }
}

public abstract class Asset<TAssetType, TAsset>: Asset where TAssetType: IAssetType<TAsset> {
    public readonly TAsset data;

    protected Asset(string name, TAsset data): base(name) {
        this.data = data;
    }

    public static string resolve_file_name() => $"{asset_root}/{TAssetType.path}/";
    public static string resolve_file_name(string filename) => $"{asset_root}/{TAssetType.path}/{filename}";

    public static IReadOnlyList<string> get_files()
        => Directory.GetFiles(resolve_file_name());

    public static IReadOnlyList<string> get_files(string directory)
        => Directory.GetFiles(resolve_file_name(directory));

    public static implicit operator TAsset(Asset<TAssetType, TAsset> asset) => asset.data;
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