using System.Collections;

namespace NetGL;

public class ReadOnlyMap<TKey, TValue>: IEnumerable<TValue> where TKey : notnull where TValue : notnull {
    protected readonly List<TValue> list = [];
    protected readonly Dictionary<TKey, TValue> dict = [];

    public TValue this[TKey key] => dict[key];
    public TValue this[int index] => list[index];
    public int length => list.Count;
    public bool contains(TKey key) => dict.ContainsKey(key);
    public bool contains(TValue key) => dict.ContainsValue(key);

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}

public class Map<TKey, TValue>: ReadOnlyMap<TKey, TValue> where TKey: notnull where TValue: notnull {
    public void add(TKey key, TValue value) {
        list.Add(value);
        dict.Add(key, value);
    }

    public void clear() {
        dict.Clear();
        list.Clear();
    }
}

public static class DictionaryExtensions {
    public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            if (value != null && value.Equals(pair.Value)) return pair.Key;

        throw new Exception("the value is not found in the dictionary");
    }
}