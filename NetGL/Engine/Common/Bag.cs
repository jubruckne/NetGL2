namespace NetGL;

using ECS;

public static class Bag {
    public static Bag<string, T> create<T>() where T: INamed
        => new Bag<string, T>(static item => item.name);
}

public class Bag<TKey, TItem> where TKey: notnull, IComparable<TKey> {
    private readonly SortedDictionary<TKey, TItem> dict;
    private readonly Func<TItem, TKey> get_key;

    public Bag(in Func<TItem, TKey> get_key) {
        this.dict = new SortedDictionary<TKey, TItem>();
        this.get_key = get_key;
    }

    public int length => dict.Count;

    public void add(in TItem item) {
        dict.Add(get_key(item), item);
        TKey k;
    }

    public TItem this[in TKey index] => dict[index];

    public bool contains(in TKey key) => dict.ContainsKey(key);
}