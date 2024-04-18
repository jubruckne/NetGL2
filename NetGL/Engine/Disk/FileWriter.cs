namespace NetGL;

public static class FileWriter {
    public static void save_to_file<T>(this Field<T> field, string filename) where T: unmanaged {
        File.WriteAllText(Asset.get_temp_file_name(filename), "jjj");

        Console.WriteLine($"{field} saved to {filename}.");
    }
}