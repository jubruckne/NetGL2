namespace NetGL;

public static class FileWriter {
    public static void save_to_file<T>(this Field<T> field, string filename) where T: unmanaged {
        File.WriteAllText(AssetManager.temp_path(filename), "jjj");

        Console.WriteLine($"{field} saved to {filename}.");f
    }
}