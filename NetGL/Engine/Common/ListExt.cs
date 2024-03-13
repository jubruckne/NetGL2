using NetGL.ECS;

namespace NetGL;

public static class ListExt {
    public static string to_print(this IEnumerable<Entity> list) {
        List<string> str = new();
        foreach (var entity in list) {
            str.Add(entity.name);
        }

        Console.WriteLine(String.Join(", ", str));

        return String.Join(", ", str);
    }

    public static IEnumerable<T> where<T>(this IEnumerable<T> list, Predicate<T> condition) {
        foreach(var item in list)
            if(condition(item)) yield return item;
    }
}